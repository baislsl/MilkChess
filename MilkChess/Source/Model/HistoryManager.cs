using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace MilkChess.Model;

public class HistoryManager
{
    private EventHandler ExecutionTurnEventHandler;

    private Color _executionTurn;
    
    public Color ExecutionTurn
    {
        get => _executionTurn;
        set
        {
            this._executionTurn = value;
            ExecutionTurnEventHandler?.Invoke(this, EventArgs.Empty);
        }
    }

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

    public HistoryManager(ILogger<HistoryManager> logger, IList<Movement> movementContainer = null)
    {
        this.Logger = logger;
        this.Movements = movementContainer ?? new List<Movement>();
        this.Reset();
    }

    public void InsertMovement(Movement movement)
    {
        this.ExecutionTurn = this.ExecutionTurn.Reverse();

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

        this.ExecutionTurn = this.ExecutionTurn.Reverse();
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

        this.ExecutionTurn = this.ExecutionTurn.Reverse();
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
        this.ExecutionTurn = Color.RED;
    }

    public void AddActiveColorListener(EventHandler eventHandler)
    {
        this.ExecutionTurnEventHandler += eventHandler;
    }
}