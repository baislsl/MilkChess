using System.Collections;
using Microsoft.Extensions.Logging;

namespace MilkChess.Source.Model;

public class HistoryManager
{
    public ChessModel InitialLayout { get; set; }

    private List<Movement> Movements;

    private int cursor;

    private static LinkedListNode<Movement> HEADER = new LinkedListNode<Movement>(null);

    private readonly ILogger Logger;

    private readonly ChessModel ChessModel;

    public Movement LastMovement
    {
        get
        {
            if (cursor >= 1) return Movements[cursor-1];
            return null;
        }
    }

    public HistoryManager(ILogger<HistoryManager> logger, ChessModel chessModel)
    {
        this.Logger = logger;
        this.ChessModel = ChessModel;
        this.Reset();
    }

    public void InsertMovement(Movement movement)
    {
        while (Movements.Any() && Movements.Count != cursor)
        {
            Movements.RemoveAt(Movements.Count - 1);
        }

        Movements.Add(movement);
        this.cursor = Movements.Count;
    }

    public Movement MoveBackward()
    {
        if (this.cursor == 0) return null;
        
        var ret = LastMovement;
        this.cursor--;
        return ret;
    }

    public Movement MoveForward()
    {
        if (this.cursor == Movements.Count)
        {
            return null;
        }

        this.cursor++;
        return this.Movements[this.cursor-1];
    }

    public IEnumerable<Movement> GetMovementHistory()
    {
        return this.Movements.Skip(1);
    }

    public Color NextColorStep()
    {
        if (LastMovement == null)
        {
            return Color.RED;
        }

        return LastMovement.ExecutedColor.Reverse();
    }

    public void Reset()
    {
        this.Movements = new List<Movement>();
        this.cursor = 0;
    }
}