using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Extensions.Logging;
using MilkChess.Model;
using MilkChess.Ucci;
using Command = MilkChess.Ucci.Command;

namespace MilkChess.Engine;

public class UcciEngineConfig
{
    public string LocalExecutableFile { get; set; }

    public Collection<string> Arguments;
}

public class UcciEngine : IUcciEngine
{
    private readonly INativeUcciEngine NativeUcciEngine;
    private readonly ILogger Logger;

    public UcciEngine(ILogger<UcciEngine> logger, UcciEngineConfig config)
    {
        this.Logger = logger;
        this.NativeUcciEngine = new LocalExecutableEngine(config.LocalExecutableFile, config.Arguments);
    }

    public UciFeedback StartUci(CancellationToken cancellationToken)
    {
        NativeUcciEngine.Initialize();

        this.SendCommand(Command.Ucci);
        
        UciFeedback feedback = new UciFeedback();
        while (!cancellationToken.IsCancellationRequested && !feedback.ReceivedUcciOk)
        {
            Logger.LogInformation("Loop");
            UcciFeedback ucciFeedback = this.ReceiveFeedback(cancellationToken);
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
        this.DiscardBufferData();
        this.SendCommand(Command.IsReady);
        UcciFeedback ucciFeedback = this.ReceiveFeedback(cancellationToken);
        if (ucciFeedback.Command != FeedbackCommand.ReadyOk)
        {
            Logger.LogError($"IsReady receive incorrect feedback {ucciFeedback}");
        }
    }

    public void SetOption(string option, string config, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Position(IChessBoardContext chessBoardContext, CancellationToken cancellationToken)
    {
        this.SendCommand($"{Command.Position} fen {chessBoardContext.BuildForsythEdwardsNotation().GetFenString()}");
    }

    public void BanMoves(IEnumerable<string> banMoves, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public BestMove Go(GoOption option, CancellationToken cancellationToken, EventHandler<InfoFeedback> onInfoReceived)
    {
        BestMove bestMove = null;

        this.DiscardBufferData();

        cancellationToken.Register(() => this.SendCommand(Command.Stop));
        this.SendCommand(this.BuildGoCommand(option));

        while (bestMove == null)
        {
            UcciFeedback feedback = this.ReceiveFeedback(CancellationToken.None);

            if (feedback == null) continue;

            if (feedback.Command == FeedbackCommand.Info)
            {
                onInfoReceived?.Invoke(this, new InfoFeedback()
                {
                                Pv = feedback.InfoPv,
                                Info = feedback.InfoDict,
                });
            }
            else if (feedback.Command == FeedbackCommand.BestMove)
            {
                bestMove = new BestMove()
                {
                                BestMovement = feedback.BestMoveBestMovement,
                                Ponder = feedback.BestMovePonder,
                                Draw = feedback.BestMoveDraw,
                                Resign = feedback.BestMoveResign,
                };
            }
        }

        Logger.LogInformation("Finished Go command");
        return bestMove;
    }

    private string BuildGoCommand(GoOption option)
    {
        StringBuilder cmd = new StringBuilder("go");

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

    public void Quit()
    {
        this.DiscardBufferData();
        this.SendCommand(Command.Quit);

        this.NativeUcciEngine.Dispose();
    }

    private void SendCommand(string command)
    {
        Logger.LogInformation($"[Send] {command}");
        this.NativeUcciEngine.SendCommand(command);
    }

    private UcciFeedback ReceiveFeedback(CancellationToken cancellationToken)
    {
        Task<UcciFeedback> task = Task.Factory.StartNew(() =>
        {
            var feedback = this.NativeUcciEngine.GetFeedback();
            if (!UcciFeedback.TryParse(feedback, out UcciFeedback ucciFeedback))
            {
                Logger.LogError($"Can't parse feedback: {feedback}");
                return null;
            }

            Logger.LogInformation($"[Received] {feedback}");
            return ucciFeedback;
        }, cancellationToken);

        task.Wait(cancellationToken);
        return task.Result;
    }

    private void DiscardBufferData()
    {
        foreach (var feedback in this.NativeUcciEngine.DiscardBufferData())
        {
            Logger.LogInformation($"[Discard] {feedback}");
        }
    }
}