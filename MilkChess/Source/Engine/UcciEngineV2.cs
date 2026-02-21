using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Ucci;
using Command = MilkChess.Ucci.Command;

namespace MilkChess.Engine;

public class UcciEngineV2 : IUcciEngineV2
{
    public EngineState State { get; set; } = EngineState.Disconnected;

    private readonly INativeUcciEngine NativeUcciEngine;
    private readonly ILogger Logger;

    private CancellationTokenSource cancellationTokenSource;
    private Task thinkingTask;


    public UcciEngineV2(ILogger<UcciEngineV2> logger, LocalExecutableEngine localExecutableEngine)
    {
        this.Logger = logger;
        this.NativeUcciEngine = localExecutableEngine;
    }

    public UciFeedback StartUci(CancellationToken cancellationToken)
    {
        NativeUcciEngine.Initialize();
        this.State = EngineState.Connected;

        this.SendCommand(Command.Ucci);

        UciFeedback feedback = new UciFeedback();
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

    public BestMove Eval(GoOption option, IChessBoard chessBoard, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void GoInfinite(GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived)
    {
        BestMove bestMove = null;

        this.SetFenPosition(chessBoardContext);

        this.SendCommand(this.BuildGoCommand(option));

        this.State = EngineState.Thinking;
        this.cancellationTokenSource = new CancellationTokenSource();

        this.thinkingTask = Task.Factory.StartNew(() =>
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                UcciFeedback feedback;
                try
                {
                    feedback = this.NativeUcciEngine.GetFeedbackAsync(this.cancellationTokenSource.Token).Result;
                }
                catch (TaskCanceledException)
                {
                    Logger.LogInformation("GetFeedbackAsync cancelled.");
                    return;
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
                    var infoFeedback = new InfoFeedback()
                    {
                        Pv = feedback.InfoPv,
                        Info = feedback.InfoDict,
                    };

                    this.PopulateIntoFeedback(infoFeedback, chessBoardContext);
                    onInfoReceived?.Invoke(infoFeedback);
                }
            }
        }, this.cancellationTokenSource.Token);
    }

    // Stop from thinking state.
    public void Stop()
    {
        if (this.State != EngineState.Thinking)
        {
            return;
        }

        this.SendCommand(Command.Stop);
        this.cancellationTokenSource?.Cancel();
        try
        {
            this.thinkingTask?.Wait();
        }
        catch (Exception e)
        {
            if (!(e is TaskCanceledException ||
                  (e is AggregateException && ((AggregateException)e).InnerExceptions.Count == 1 &&
                   ((AggregateException)e).InnerExceptions[0].InnerException is TaskCanceledException)))
            {
                Logger.LogError(e, "Unexpected engine stop error");
            }
        }
        finally
        {
            this.thinkingTask = null;
            this.cancellationTokenSource = null;
            this.State = EngineState.Ready;
        }
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

    public void SetFenPosition(IChessBoardContext chessBoardContext)
    {
        this.SendCommand($"{Command.Position} fen {chessBoardContext.BuildForsythEdwardsNotation().GetFenString()}");
    }

    private void PopulateIntoFeedback(InfoFeedback feedback, IChessBoardContext chessBoardContext)
    {
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
            StringBuilder predict = new StringBuilder();
            ChessModel tmpChessModel = new ChessModel(chessBoardContext);

            for (int i = 0; i < feedback.Pv.Count; i++)
            {
                if (!MovementExtension.ParseMovementFromUnifyString(feedback.Pv[i], out Movement movement))
                {
                    Logger.LogError("Error parsing movement {}", feedback.Pv[i]);
                    break;
                }

                tmpChessModel.PopulateMovement(movement);

                predict.Append(" ").Append(movement.ChineseViewableString);
                tmpChessModel.ApplyMovement(movement);
                feedback.Movements.Add(movement);
            }

            feedback.ViewableMovementList = predict.ToString();
        }

        Logger.LogInformation("Finished HandleInfoFeedback");
    }
}

public enum EngineState
{
    Disconnected,
    Connected,
    Ready,
    Thinking,
}

internal interface IUcciEngineV2
{
    public EngineState State { get; }

    public UciFeedback StartUci(CancellationToken cancellationToken);

    public void WaitToReady(CancellationToken cancellationToken);

    public void SetOption(string option, string config, CancellationToken cancellationToken);

    public void BanMoves(IEnumerable<string> banMoves, CancellationToken cancellationToken);

    public BestMove Eval(GoOption option, IChessBoard chessBoard, CancellationToken cancellationToken);

    public void GoInfinite(GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived);

    // Stop from thinking state.
    public void Stop();

    public void Quit();
}