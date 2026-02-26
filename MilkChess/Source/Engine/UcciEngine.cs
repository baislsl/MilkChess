using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Ucci;
using Command = MilkChess.Ucci.Command;

namespace MilkChess.Engine;

public partial class UcciEngine : ObservableObject, IUcciEngine
{
    [ObservableProperty] private EngineState state = EngineState.Disconnected;

    private readonly INativeUcciEngine NativeUcciEngine;
    private readonly MilkChessConfig Config;
    private readonly ILogger Logger;

    private CancellationTokenSource cancellationTokenSource;
    private Task thinkingTask;

    public UcciEngine(ILogger<UcciEngine> logger, LocalExecutableEngine localExecutableEngine,
        IOptions<MilkChessConfig> config)
    {
        this.Logger = logger;
        this.NativeUcciEngine = localExecutableEngine;
        this.Config = config.Value;
    }

    public StartUciFeedback StartUci(CancellationToken cancellationToken)
    {
        NativeUcciEngine.Initialize();
        this.State = EngineState.Connected;

        this.SendCommand(Command.Ucci);

        StartUciFeedback feedback = new StartUciFeedback();
        while (!cancellationToken.IsCancellationRequested && !feedback.ReceivedUcciOk)
        {
            Logger.LogInformation("Loop");
            UcciFeedback ucciFeedback = this.NativeUcciEngine.GetFeedbackAsync(cancellationToken).Result;
            if (ucciFeedback == null) continue;

            if (ucciFeedback.Command == FeedbackCommand.UciOk)
            {
                feedback.ReceivedUcciOk = true;
            }
            else if (ucciFeedback.Command == FeedbackCommand.Id)
            {
                feedback.ID[ucciFeedback.IdName] = ucciFeedback.IdValue;
            }
            else if (ucciFeedback.Command == FeedbackCommand.Info)
            {
                // TODO: skip
            }
        }

        return feedback;
    }

    public void WaitToReady(CancellationToken cancellationToken)
    {
        this.SendCommand(Command.IsReady);
        while (!cancellationToken.IsCancellationRequested)
        {
            UcciFeedback ucciFeedback = this.NativeUcciEngine.GetFeedbackAsync(cancellationToken).Result;
            if (ucciFeedback?.Command == FeedbackCommand.ReadyOk)
            {
                Logger.LogError("Engine IsReady");
                this.State = EngineState.Ready;
                return;
            }
        }

        throw new Exception("IsReady receive no feedback");
    }

    public void SetOption(string name, string value, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();

        builder.Append(Command.SetOption)
            .Append(" name ")
            .Append(name);

        if (!string.IsNullOrEmpty(value))
        {
            builder.Append(" value ").Append(value);
        }

        this.SendCommand(builder.ToString());
    }

    public void BanMoves(IEnumerable<string> banMoves, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    // Execute forever at a new thread.
    public void ExecuteForever(
        GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived,
        Action<BestMove> onBestMovementReceived)
    {
        lock (this)
        {
            this.ThrowIfNotReady();
            this.ExecuteForeverInternal(option, chessBoardContext, onInfoReceived, onBestMovementReceived);
        }
    }

    private void ExecuteForeverInternal(
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
                this.SetFenPosition(chessBoardContext);
                this.SendCommand(this.BuildGoCommand(option));

                var bestmove = await this
                    .ListenEngineFeedbackAsync(CancellationToken.None, chessBoardContext, onInfoReceived)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                onBestMovementReceived?.Invoke(bestmove);
            }
        }, cancellationToken);
    }

    // Execute one step at a new thread.
    public void ExecuteOnce(
        GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onFeedbackReceived,
        Action<BestMove> onBestMovementReceived)
    {
        lock (this)
        {
            this.ThrowIfNotReady();
            this.GoInternal(option, chessBoardContext, onFeedbackReceived, onBestMovementReceived);
        }
    }

    // Go infinite for current chess board at a new thread.
    public void GoInfinite(GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onFeedbackReceived)
    {
        lock (this)
        {
            this.ThrowIfNotReady();
            this.GoInternal(option, chessBoardContext, onFeedbackReceived, null);
        }
    }

    private void GoInternal(GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived,
        Action<BestMove> onBestMovementReceived)
    {
        this.State = EngineState.Thinking;

        this.SetFenPosition(chessBoardContext);
        this.SendCommand(this.BuildGoCommand(option));

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
        while (!cancellationToken.IsCancellationRequested)
        {
            UcciFeedback feedback;
            try
            {
                feedback = await this.NativeUcciEngine.GetFeedbackAsync(cancellationToken).ConfigureAwait(false);
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
                onInfoReceived.Invoke(this.ParseIntoFeedback(feedback, chessBoardContext));
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
    public void Stop()
    {
        lock (this)
        {
            this.StopInternal();
        }
    }

    private void StopInternal()
    {
        if (this.State != EngineState.Thinking)
        {
            return;
        }

        this.SendCommand(Command.Stop);

        // TODO: firstly stop or cancel?
        this.cancellationTokenSource.Cancel();

        try
        {
            this.thinkingTask.Wait();
        }
        catch (Exception ex) when (ExceptionUtil.GetInnerMostException(ex) is TaskCanceledException)
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

    public void Quit()
    {
        this.SendCommand(Command.Quit);

        this.NativeUcciEngine.Dispose();
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

    private void SendCommand(string command)
    {
        Logger.LogInformation($"[Send] {command}");
        this.NativeUcciEngine.SendCommand(command);
    }

    private void SetFenPosition(IChessBoardContext chessBoardContext)
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

        this.SendCommand(command.ToString());
    }

    private InfoFeedback ParseIntoFeedback(UcciFeedback ucciFeedback, IChessBoardContext chessBoardContext)
    {
        if (ucciFeedback.Command != FeedbackCommand.Info)
        {
            throw new Exception("Not info feedback command");
        }

        var feedback = new InfoFeedback()
        {
            Pv = ucciFeedback.InfoPv,
            Info = ucciFeedback.InfoDict,
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