using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace MilkChess.Model;

public class HistoryManager
{
    public ChessModel InitialLayout { get; set; }

    public Color NextColorStep { get; set; }

    public IList<Movement> Movements { get; private set; }

    public int Cursor;

    private readonly ILogger Logger;

    public Movement LastMovement
    {
        get
        {
            if (Cursor >= 1) return Movements[Cursor - 1];
            return null;
        }
    }

    public HistoryManager(ILogger<HistoryManager> logger, Collection<Movement> movementContainer = null)
    {
        this.Logger = logger;
        this.Movements = movementContainer ?? (IList<Movement>) new List<Movement>();
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
        if (this.Cursor == 0) return null;

        this.NextColorStep = this.NextColorStep.Reverse();
        var ret = LastMovement;
        this.Cursor--;
        return ret;
    }

    public Movement MoveForward()
    {
        if (this.Cursor == Movements.Count)
        {
            return null;
        }

        this.NextColorStep = this.NextColorStep.Reverse();
        this.Cursor++;
        return this.Movements[this.Cursor - 1];
    }

    public IEnumerable<Movement> GetMovementHistory()
    {
        return this.Movements;
    }

    public void Reset()
    {
        this.Movements.Clear();
        this.Cursor = 0;
        this.NextColorStep = Color.RED;
    }
}