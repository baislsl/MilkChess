using Microsoft.Extensions.Logging;
using MilkChess.Model;

namespace MilkChess.Engine;

internal interface INativeUcciEngine : IDisposable
{
    void Initialize();

    IEnumerable<string> DiscardBufferData();

    public void SendCommand(string command);

    public string GetFeedback();
}

internal interface IUcciEngine
{
    public UciFeedback StartUci(CancellationToken cancellationToken);

    public void WaitToReady(CancellationToken cancellationToken);

    public void SetOption(string option, string config, CancellationToken cancellationToken);

    public void Position(IChessBoardHandler chessBoardHandler, CancellationToken cancellationToken);

    public void BanMoves(IEnumerable<string> banMoves, CancellationToken cancellationToken);

    public BestMove Go(GoOption option, CancellationToken cancellationToken, EventHandler<InfoFeedback> onInfoReceived);

    public void Quit();
}

public class GoOption
{
    public bool? Ponder { get; set; }
    public bool? Draw { get; set; }
    public int? Depth { get; set; }
    public int? Nodes { get; set; }
    public int? Time { get; set; }
    public int? MovesToGo { get; set; }
    public int? Increment { get; set; }
    public int? OppTime { get; set; }
    public int? OppMovesToGo { get; set; }
    public int? OppIncrement { get; set; }
}

public class BestMove
{
    public string BestMovement { get; set; }
    public string Ponder { get; set; }
    public bool Draw { get; set; }
    public bool Resign { get; set; }
}

public class InfoFeedback
{
    public List<string> Pv { get; set; }
    public Dictionary<string, string> Info { get; set; }

    public InfoFeedback()
    {
        this.Pv = new List<string>();
        this.Info = new Dictionary<string, string>();
    }
}

public class TimeNodeFeedback : InfoFeedback
{
    public string Time { get; set; }
    public string Nodes { get; set; }
}

public class DepthFeedback : InfoFeedback
{
    public string Depth { get; set; }
    public string Score { get; set; }
    public List<string> Pv { get; set; }
}

public class CurrMoveFeedback : InfoFeedback
{
    public string CurrMove { get; set; }
}

public class MessageFeedback : InfoFeedback
{
    public string NotifyMessage { get; set; }
}

public class UciFeedback
{
    public Dictionary<string, string> ID { get; set; }
    public List<string> Options { get; set; }

    public UciFeedback()
    {
        this.ID = new Dictionary<string, string>();
        this.Options = new List<string>();
    }
}