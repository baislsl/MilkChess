using System.Collections;
using Microsoft.Extensions.Logging;

namespace MilkChess.Model;

public class HistoryManager
{
    public ChessModel InitialLayout { get; set; }

    public Color NextColorStep { get; set; }

    private List<Movement> Movements;

    private int Cursor;

    private static LinkedListNode<Movement> HEADER = new LinkedListNode<Movement>(null);

    private readonly ILogger Logger;

    private readonly ChessModel ChessModel;

    public Movement LastMovement
    {
        get
        {
            if (Cursor >= 1) return Movements[Cursor - 1];
            return null;
        }
    }

    public HistoryManager(ILogger<HistoryManager> logger)
    {
        this.Logger = logger;
        this.Reset();
    }

    public void InsertMovement(Movement movement)
    {
        this.NextColorStep = this.NextColorStep.Reverse();

        while (Movements.Any() && Movements.Count != Cursor)
        {
            Movements.RemoveAt(Movements.Count - 1);
        }

        Movements.Add(movement);
        this.Cursor = Movements.Count;
    }

    public Movement MoveBackward()
    {
        this.NextColorStep = this.NextColorStep.Reverse();

        if (this.Cursor == 0) return null;

        var ret = LastMovement;
        this.Cursor--;
        return ret;
    }

    public Movement MoveForward()
    {
        this.NextColorStep = this.NextColorStep.Reverse();

        if (this.Cursor == Movements.Count)
        {
            return null;
        }

        this.Cursor++;
        return this.Movements[this.Cursor - 1];
    }

    public IEnumerable<Movement> GetMovementHistory()
    {
        return this.Movements;
    }

    public void Reset()
    {
        this.Movements = new List<Movement>();
        this.Cursor = 0;
        this.NextColorStep = Color.RED;
    }
}