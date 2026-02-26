using Microsoft.Extensions.Logging;
using MilkChess.Model;
using MilkChess.Protocol;

namespace MilkChessTest;

public class MockChessModel : IChessBoardContext
{
    private Chess[,] Chesses = new Chess[Constants.ChessBoardHeight, Constants.ChessBoardWidth];

    public MockChessModel(IServiceProvider serviceProvider)
    {
        HistoryManager = new HistoryManager(serviceProvider.GetRequiredService<ILogger<HistoryManager>>());
    }

    public HistoryManager HistoryManager { get; }

    public Chess GetChessAtPosition(Position position)
    {
        return Chesses[position.Y, position.X];
    }


    public void SetChessAtPosition(Chess chess, Position position, out Chess removedChess)
    {
        removedChess = GetChessAtPosition(position);
        Chesses[position.Y, position.X] = chess;
    }

    public virtual void ApplyMovement(Movement movement)
    {
        SetChessAtPosition(null, movement.From, out _);
        SetChessAtPosition(movement.MovedChess, movement.To, out _);
    }

    public virtual void RevertMovement(Movement movement)
    {
        SetChessAtPosition(movement.MovedChess, movement.From, out _);
        SetChessAtPosition(movement.RemovedChess, movement.To, out _);
    }

    public virtual void MoveChess(Position from, Position to, out Chess removedChess)
    {
        Chess chess = this.GetChessAtPosition(from);
        this.SetChessAtPosition(chess, to, out removedChess);
    }

    public void Reset()
    {
    }
}