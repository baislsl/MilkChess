using Microsoft.Extensions.Logging;
using MilkChess.Model;

namespace MilkChess.Engine;

public class UcciEngineConfig
{
    public string LocalExecutableFile { get; set; }
}

public class UcciEngine : IUcciEngine
{
    private INativeUcciEngine NativeUcciEngine;
    private ILogger Logger;

    public UcciEngine(ILogger logger, UcciEngineConfig config)
    {
        this.Logger = logger;
        this.NativeUcciEngine = new LocalExecutableEngine(logger, config.LocalExecutableFile, null);
    }

    public UciFeedback StartUci()
    {
        throw new NotImplementedException();
    }

    public void WaitToReady()
    {
        throw new NotImplementedException();
    }

    public void SetOption(string option, string config)
    {
        throw new NotImplementedException();
    }

    public void Position(IChessBoardHandler chessBoardHandler)
    {
        throw new NotImplementedException();
    }

    public void BanMoves(IEnumerable<string> banMoves)
    {
        throw new NotImplementedException();
    }

    public BestMove Go(GoOption option, CancellationToken cancellationToken, EventHandler<InfoFeedback> onInfoReceived)
    {
        throw new NotImplementedException();
    }

    public void Quit()
    {
        throw new NotImplementedException();
    }
}