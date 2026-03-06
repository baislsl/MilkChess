using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MilkChess.Protocol;

namespace MilkChess.Model;

public partial class HistoryManager : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExecutionTurnColor))]
    private Color _executionTurn;

    public Microsoft.Maui.Graphics.Color ExecutionTurnColor =>
        this.ExecutionTurn == Model.Color.RED ? Colors.Red : Colors.Black;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastMovement))]
    [NotifyPropertyChangedFor(nameof(NextMovement))]
    private ObservableCollection<Movement> _movements;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastMovement))]
    [NotifyPropertyChangedFor(nameof(NextMovement))]
    private int _cursor;

    private readonly ILogger Logger;

    public Movement LastMovement
    {
        get
        {
            if (Cursor >= 1) return Movements[Cursor - 1];
            return null;
        }
    }

    public Movement NextMovement
    {
        get
        {
            if (Cursor < Movements.Count)
            {
                return Movements[Cursor];
            }

            return null;
        }
    }

    public HistoryManager(ILogger<HistoryManager> logger)
    {
        this.Logger = logger;
        this.Movements = new ObservableCollection<Movement>();
        this.Reset();
    }

    public void InsertMovement(Movement movement)
    {
        this.ExecutionTurn = this.ExecutionTurn.Reverse();

        movement.ActionCount = this.Cursor + 1;

        while (Movements.Any() && Movements.Count != Cursor)
        {
            Movements.RemoveAt(Movements.Count - 1);
        }

        Movements.Add(movement);
        this.Cursor = Movements.Count;
    }

    public bool CanMoveBackward()
    {
        return this.Cursor != 0;
    }

    public Movement MoveBackward()
    {
        if (this.Cursor == 0) return null;

        this.ExecutionTurn = this.ExecutionTurn.Reverse();
        var ret = LastMovement;
        this.Cursor--;
        return ret;
    }

    public bool CanMoveForward()
    {
        return this.Cursor < Movements.Count;
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

    public int GetCursor()
    {
        return this.Cursor;
    }

    public IEnumerable<Movement> GetMovementHistory()
    {
        for (int cursor = this.Cursor - 1; cursor >= 0; cursor--)
        {
            yield return this.Movements[cursor];
        }
    }

    public void Reset()
    {
        this.Movements.Clear();
        this.Cursor = 0;
        this.ExecutionTurn = Color.RED;
    }

    public void ExchangeTurn()
    {
        this.ExecutionTurn = this.ExecutionTurn.Reverse();
        this.Movements.Clear();
        this.Cursor = 0;
    }

    partial void OnCursorChanged(int oldValue, int newValue)
    {
        if (oldValue - 1 < this.Movements.Count && oldValue > 0)
        {
            this.Movements[oldValue - 1].IsSelected = false;
        }

        if (newValue > 0)
        {
            this.Movements[newValue - 1].IsSelected = true;
        }

        OnPropertyChanged();
    }
}