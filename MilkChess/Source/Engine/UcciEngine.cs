using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using MilkChess.Model;
using Command = MilkChess.Ucci.Command;

namespace MilkChess.Engine;

public class UcciEngineConfig
{
    public string LocalExecutableFile { get; set; }

    public Collection<string> Arguments;
}

public class UcciEngine : IUcciEngine
{
    private INativeUcciEngine NativeUcciEngine;
    private ILogger Logger;

    public UcciEngine(ILogger logger, UcciEngineConfig config)
    {
        this.Logger = logger;
        this.NativeUcciEngine = new LocalExecutableEngine(logger, config.LocalExecutableFile, config.Arguments);
    }

    public UciFeedback StartUci(CancellationToken cancellationToken)
    {
        NativeUcciEngine.Initialize();

        this.ClearBufferData();
        this.SendCommand(Command.Ucci);

        UciFeedback feedback = new UciFeedback();
        bool getUciFeedback = false;

        while (!cancellationToken.IsCancellationRequested && !getUciFeedback)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                UcciFeedback ucciFeedback = this.ReceiveFeedback();
                if (ucciFeedback == null) return;

                if (ucciFeedback.Command == FeedbackCommand.UciOk)
                {
                    getUciFeedback = true;
                }
                else if (ucciFeedback.Command == FeedbackCommand.Id)
                {
                    feedback.ID[ucciFeedback.IdName] = ucciFeedback.IdValue;
                }
                else if (ucciFeedback.Command == FeedbackCommand.Info)
                {
                    // TODO: skip
                }
            });

            t.Wait(cancellationToken);
        }

        return feedback;
    }

    public void WaitToReady(CancellationToken cancellationToken)
    {
        this.ClearBufferData();
        this.SendCommand(Command.IsReady);
        UcciFeedback ucciFeedback = this.ReceiveFeedback();
        if (ucciFeedback.Command != FeedbackCommand.ReadyOk)
        {
            Logger.LogError($"IsReady receive incorrect feedback {ucciFeedback}");
        }
    }

    public void SetOption(string option, string config, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Position(IChessBoardHandler chessBoardHandler, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void BanMoves(IEnumerable<string> banMoves, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public BestMove Go(GoOption option, CancellationToken cancellationToken, EventHandler<InfoFeedback> onInfoReceived)
    {
        throw new NotImplementedException();
    }

    public void Quit()
    {
        this.ClearBufferData();
        this.SendCommand(Command.Quit);

        this.NativeUcciEngine.Dispose();
    }

    private void SendCommand(string cmd)
    {
        this.NativeUcciEngine.SendCommand(cmd);
    }

    private UcciFeedback ReceiveFeedback()
    {
        var feedback = this.NativeUcciEngine.GetFeedback();
        if (!UcciFeedback.TryParse(feedback, out UcciFeedback ucciFeedback))
        {
            Logger.LogError($"Can't parse feedback {feedback}");
            return null;
        }

        return ucciFeedback;
    }

    private void ClearBufferData()
    {
        this.NativeUcciEngine.DiscardBufferData();
    }
}