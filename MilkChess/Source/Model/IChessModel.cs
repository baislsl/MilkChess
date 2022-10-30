namespace MilkChess.Model;

public interface IChessBoardHandler
{
    public HistoryManager HistoryManager { get; }

    public Chess GetChessAtPosition(Position position);

    public void SetChessAtPosition(Chess chess, Position position, out Chess removedChess);
}