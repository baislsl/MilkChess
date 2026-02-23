using CommunityToolkit.Mvvm.ComponentModel;
using MilkChess.Model;
using Color = MilkChess.Model.Color;

namespace MilkChess.UserInterface;

public partial class MovementIndicator : ObservableObject
{
    private IChessBoard ChessBoard { get; set; }

    [ObservableProperty] private CursorSelector _from;

    [ObservableProperty] private CursorSelector _to;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(UISource))]
    private Movement _movement;

    public string UISource => this.Movement?.ExecutedColor == Color.BLACK
        ? UIConstants.BlackMovementCursorResource
        : UIConstants.RedMovementCursorResource;

    public MovementIndicator(IChessBoard chessBoard)
    {
        this.ChessBoard = chessBoard;
        this.From = new CursorSelector(chessBoard, null);
        this.To = new CursorSelector(chessBoard, null);
    }

    public void SetMovement(Movement movement)
    {
        this.Movement = movement;

        if (movement != null)
        {
            From.Position = movement.From;
            To.Position = movement.To;
        }
        else
        {
            this.Reset();
        }
    }

    public void Reset()
    {
        From.Position = null;
        To.Position = null;
        this.Movement = null;
    }

    public void Refresh()
    {
        this.From.Refresh();
        this.To.Refresh();
    }
}