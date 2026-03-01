using System.Text;
using Microsoft.Extensions.Logging;
using MilkChess.Model;

namespace MilkChess.Engine;

internal interface INativeUciEngine : IDisposable
{
    public string Name { get; }

    void Initialize();

    public Task SendCommandAsync(string command);

    public Task<UciFeedback> GetFeedbackAsync(CancellationToken cancellationToken);
}

public enum EngineState
{
    Disconnected,
    Connected,
    Ready,
    Thinking,
}

internal interface IUciEngine
{
    public EngineState State { get; }

    public Task<StartUciFeedback> StartUciAsync(CancellationToken cancellationToken);

    public Task WaitToReadyAsync(CancellationToken cancellationToken);

    public Task SetOptionAsync(string option, string config, CancellationToken cancellationToken);

    public Task BanMovesAsync(IEnumerable<string> banMoves, CancellationToken cancellationToken);

    public Task ExecuteForeverAsync(
        GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived,
        Action<BestMove> onBestMovementReceived);

    public Task ExecuteOnceAsync(
        GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived,
        Action<BestMove> onBestMovementReceived);

    public Task GoInfiniteAsync(GoOption option,
        IChessBoardContext chessBoardContext,
        Action<InfoFeedback> onInfoReceived);

    // Stop from thinking state.
    public Task StopAsync();

    public Task QuitAsync();
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

    public int Score;

    public int Mate;

    public int Depth;

    public List<Movement> Movements;

    public string ViewableMovementList;

    public InfoFeedback()
    {
        this.Pv = new List<string>();
        this.Info = new Dictionary<string, string>();
        this.Movements = new List<Movement>();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var keyPair in Info)
        {
            sb.Append($"{keyPair.Key}:{keyPair.Value} ");
        }

        foreach (var movement in Pv)
        {
            sb.Append(" ").Append(movement);
        }

        return sb.ToString();
    }
}

public class StartUciFeedback
{
    public Dictionary<string, string> ID { get; set; }
    public List<string> Options { get; set; }

    public bool ReceivedUciOk { get; set; }

    public StartUciFeedback()
    {
        this.ID = new Dictionary<string, string>();
        this.Options = new List<string>();
    }
}