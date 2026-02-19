namespace MilkChess.Model;

public interface IChessBoardContext : IChessBoard
{
    public HistoryManager HistoryManager { get; }
}

public interface IChessBoard
{
    public Chess GetChessAtPosition(Position position);

    public void SetChessAtPosition(Chess chess, Position position, out Chess removedChess);

    public void ApplyMovement(Movement movement);

    public void RevertMovement(Movement movement);

    public void MoveChess(Position from, Position to, out Chess removedChess);

    public void Reset();
}