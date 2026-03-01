using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Protocol;
using Command = MilkChess.Protocol.Command;

namespace MilkChess.Engine;

public partial class UciEngine : ObservableObject, IUciEngine
{
    [ObservableProperty] private EngineState state = EngineState.Disconnected;

    private readonly INativeUciEngine NativeUciEngine;
    private readonly MilkChessConfig Config;
    private readonly ILogger Logger;

    private CancellationTokenSource cancellationTokenSource;
    private Task thinkingTask;

    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    public string Name => this.NativeUciEngine.Name;

    public UciEngine(ILogger<UciEngine> logger, LocalExecutableEngine localExecutableEngine,
        MilkChessConfig config)
    {
        this.Logger = logger;
        this.NativeUciEngine = localExecutableEngine;
        this.Config = config;
    }

    public async Task<StartUciFeedback> StartUciAsync(CancellationToken cancellationToken)
    {
        NativeUciEngine.Initialize();
        this.State = EngineState.Connected;

        await this.SendCommandAsync(Command.Uci).ConfigureAwait(false);

        StartUciFeedback feedback = new StartUciFeedback();
        while (!cancellationToken.IsCancellationRequested && !feedback.ReceivedUciOk)
        {
            Logger.LogInformation("Loop");
            UciFeedback UciFeedback =
                await this.NativeUciEngine.GetFeedbackAsync(cancellationToken).ConfigureAwait(false);
            if (UciFeedback == null) continue;

            if (UciFeedback.Command == FeedbackCommand.UciOk)
            {
                feedback.ReceivedUciOk = true;
            }
            else if (UciFeedback.Command == FeedbackCommand.Id)
            {
                feedback.ID[UciFeedback.IdName] = UciFeedback.IdValue;
            }
            else if (UciFeedback.Command == FeedbackCommand.Info)
            {
                // TODO: skip
            }
        }

        return feedback;
    }

    public async Task WaitToReadyAsync(CancellationToken cancellationToken)
    {
        await this.SendCommandAsync(Command.IsReady).ConfigureAwait(false);

        while (!cancellationToken.IsCancellationRequested)
        {
            UciFeedback UciFeedback =
                await this.NativeUciEngine.GetFeedbackAsync(cancellationToken).ConfigureAwait(false);
            if (UciFeedback?.Command == FeedbackCommand.ReadyOk)
            {
                Logger.LogError("Engine IsReady");
                this.State = EngineState.Ready;
                return;
            }
        }

        throw new Exception("IsReady receive no feedback");
    }

    public async Task SetOptionAsync(string name, string value, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();

        builder.Append(Command.SetOption)
            .Append(" name ")
            .Append(name);

        if (!string.IsNullOrEmpty(value))
        {
            builder.Append(" value ").Append(value);
        }

        await this.SendCommandAsync(builder.ToString()).ConfigureAwait(false);
    }

    public Task BanMovesAsync(IEnumerable<string> banMoves, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    // Execute forever at a new thread.
    public async Task ExecuteForeverAsync(
        GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived,
        Action<BestMove> onBestMovementReceived)
    {
        await this.ThreadSafeExecute(async () =>
        {
            this.ThrowIfNotReady();
            await this.ExecuteForeverInternalAsync(option, chessBoardContext, onInfoReceived, onBestMovementReceived);
        });
    }

    private Task ExecuteForeverInternalAsync(
        GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived,
        Action<BestMove> onBestMovementReceived)
    {
        this.State = EngineState.Thinking;
        this.cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = this.cancellationTokenSource.Token;

        this.thinkingTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await this.SetFenPositionAsync(chessBoardContext).ConfigureAwait(false);
                await this.SendCommandAsync(this.BuildGoCommand(option)).ConfigureAwait(false);

                var bestmove = await this
                    .ListenEngineFeedbackAsync(CancellationToken.None, chessBoardContext, onInfoReceived)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                onBestMovementReceived?.Invoke(bestmove);
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    // Execute one step at a new thread.
    public async Task ExecuteOnceAsync(
        GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onFeedbackReceived,
        Action<BestMove> onBestMovementReceived)
    {
        await this.ThreadSafeExecute(async () =>
        {
            this.ThrowIfNotReady();
            await this.GoInternalAsync(option, chessBoardContext, onFeedbackReceived, onBestMovementReceived);
        });
    }

    // Go infinite for current chess board at a new thread.
    public async Task GoInfiniteAsync(GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onFeedbackReceived)
    {
        await this.ThreadSafeExecute(async () =>
        {
            this.ThrowIfNotReady();
            await this.GoInternalAsync(option, chessBoardContext, onFeedbackReceived, null);
        });
    }

    private async Task GoInternalAsync(GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived,
        Action<BestMove> onBestMovementReceived)
    {
        this.State = EngineState.Thinking;

        await this.SetFenPositionAsync(chessBoardContext);
        await this.SendCommandAsync(this.BuildGoCommand(option));

        this.cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = this.cancellationTokenSource.Token;
        this.thinkingTask = Task.Run(async () =>
        {
            var bestmove = await this
                .ListenEngineFeedbackAsync(CancellationToken.None, chessBoardContext, onInfoReceived)
                .ConfigureAwait(false);

            // If user stop the engine manually, throw and no need to execute onBestMovementReceived.
            cancellationToken.ThrowIfCancellationRequested();

            // !IsCancellationRequested means engine return best move and stop by itself
            onBestMovementReceived?.Invoke(bestmove);

            this.State = EngineState.Ready;
            this.cancellationTokenSource = null;
            this.thinkingTask = null;
        }, cancellationToken);
    }

    // return null if canceled
    private async Task<BestMove> ListenEngineFeedbackAsync(
        CancellationToken cancellationToken,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived)
    {
        bool isMateReceived = false;
        while (!cancellationToken.IsCancellationRequested)
        {
            UciFeedback feedback;
            try
            {
                feedback = await this.NativeUciEngine.GetFeedbackAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ExceptionUtil.GetInnerMostException(ex) is TaskCanceledException)
            {
                Logger.LogInformation("GetFeedbackAsync cancelled.");
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetFeedbackAsync error");
                throw;
            }

            if (feedback == null)
            {
                // Received Invalid feedback, sleep to avoid stuck.
                Logger.LogWarning("Getting invalid feedback");
                Thread.Sleep(20);
                continue;
            }

            if (feedback.Command == FeedbackCommand.Info && feedback.InfoPv?.Count > 0)
            {
                if (!isMateReceived)
                {
                    onInfoReceived.Invoke(this.ParseIntoFeedback(feedback, chessBoardContext));
                }

                // Terminate info update if mate.
                isMateReceived = feedback.InfoDict.ContainsKey("mate");
            }
            else if (feedback.Command == FeedbackCommand.BestMove)
            {
                return new BestMove()
                {
                    BestMovement = feedback.BestMoveBestMovement,
                    Ponder = feedback.BestMovePonder,
                    Draw = feedback.BestMoveDraw,
                    Resign = feedback.BestMoveResign,
                };
            }
        }

        return null;
    }

    // Stop from thinking state.
    public async Task StopAsync()
    {
        await this.ThreadSafeExecute(async () => { await this.StopInternalAsync(); });
    }

    private async Task StopInternalAsync()
    {
        if (this.State != EngineState.Thinking)
        {
            return;
        }

        await this.SendCommandAsync(Command.Stop).ConfigureAwait(false);

        // TODO: firstly stop or cancel?
        await this.cancellationTokenSource.CancelAsync().ConfigureAwait(false);

        try
        {
            await this.thinkingTask.ConfigureAwait(false);
        }
        catch (Exception ex) when (ExceptionUtil.GetInnerMostException(ex) is TaskCanceledException
                                       or OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected engine stop error");
            throw;
        }

        this.thinkingTask = null;
        this.cancellationTokenSource = null;

        this.State = EngineState.Ready;
    }

    public async Task QuitAsync()
    {
        await this.SendCommandAsync(Command.Quit).ConfigureAwait(false);

        this.NativeUciEngine.Dispose();
    }

    private async Task ThreadSafeExecute(Func<Task> task)
    {
        await task();
        // await semaphore.WaitAsync();
        // try
        // {
        //     await task();
        // }
        // finally
        // {
        //     semaphore.Release();
        // }
    }

    private string BuildGoCommand(GoOption option)
    {
        StringBuilder cmd = new StringBuilder(Command.Go);

        if (option.Ponder.HasValue)
        {
            cmd.Append(" ponder");
        }
        else if (option.Draw.HasValue)
        {
            cmd.Append(" draw");
        }

        if (option.Depth.HasValue)
        {
            if (option.Depth >= 0)
            {
                cmd.Append(" depth ").Append(option.Depth.Value);
            }
            else // infinite
            {
                cmd.Append(" depth ").Append("infinite");
            }
        }

        if (option.Nodes.HasValue)
        {
            cmd.Append(" nodes ").Append(option.Nodes.Value);
        }

        if (option.Time.HasValue)
        {
            cmd.Append(" time ").Append(option.Time.Value);

            if (option.MovesToGo.HasValue) cmd.Append(" movestogo ").Append(option.MovesToGo.Value);
            if (option.Increment.HasValue) cmd.Append(" increment ").Append(option.Increment.Value);
            if (option.OppTime.HasValue) cmd.Append(" opptime ").Append(option.OppTime.Value);
            if (option.OppMovesToGo.HasValue) cmd.Append(" oppmovestogo ").Append(option.OppMovesToGo.Value);
            if (option.OppIncrement.HasValue) cmd.Append(" oppincrement ").Append(option.OppIncrement.Value);
        }

        return cmd.ToString();
    }

    private async Task SendCommandAsync(string command)
    {
        Logger.LogInformation($"[Send] {command}");
        await this.NativeUciEngine.SendCommandAsync(command).ConfigureAwait(false);
    }

    private async Task SetFenPositionAsync(IChessBoardContext chessBoardContext)
    {
        var (chessBoard, movements, executionTurn) =
            chessBoardContext.HistoryManager.GetUnrevertibleChessBoardTrace(chessBoardContext);

        StringBuilder command = new($"{Command.Position} fen ");

        command.Append(chessBoard.BuildForsythEdwardsNotation(executionTurn).GetFenString());

        if (movements.Count != 0)
        {
            command.Append(" moves");
            movements.Reverse();
            foreach (var movement in movements)
            {
                command.Append(" ").Append(movement.ToUnifyString());
            }
        }

        await this.SendCommandAsync(command.ToString()).ConfigureAwait(false);
    }

    private InfoFeedback ParseIntoFeedback(UciFeedback UciFeedback, IChessBoardContext chessBoardContext)
    {
        if (UciFeedback.Command != FeedbackCommand.Info)
        {
            throw new Exception("Not info feedback command");
        }

        var feedback = new InfoFeedback()
        {
            Pv = UciFeedback.InfoPv,
            Info = UciFeedback.InfoDict,
        };

        Logger.LogDebug("[PopulateIntoFeedback] {}", feedback);

        if (feedback.Info.ContainsKey("score") &&
            int.TryParse(feedback.Info["score"], out feedback.Score))
        {
            if (chessBoardContext.HistoryManager.ExecutionTurn == Model.Color.BLACK)
            {
                feedback.Score = -feedback.Score;
            }
        }

        if (feedback.Info.ContainsKey("depth"))
        {
            int.TryParse(feedback.Info["depth"], out feedback.Depth);
        }

        if (feedback.Pv?.Count > 0)
        {
            var predict = new StringBuilder();
            var tmpChessModel = new ChessModel(chessBoardContext);

            for (int i = 0; i < feedback.Pv.Count; i++)
            {
                if (!tmpChessModel.ParseUnifyStringMovement(feedback.Pv[i], out Movement movement))
                {
                    Logger.LogError("Error parsing movement {}", feedback.Pv[i]);
                    break;
                }

                predict.Append(" ").Append(movement.ChineseViewableString);
                tmpChessModel.ApplyMovement(movement);
                feedback.Movements.Add(movement);
            }

            feedback.ViewableMovementList = predict.ToString();
        }

        return feedback;
    }

    private void ThrowIfNotReady()
    {
        if (this.State != EngineState.Ready)
        {
            throw new Exception("Engine is not ready");
        }
    }
}