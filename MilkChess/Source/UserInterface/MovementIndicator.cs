using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;
using MilkChess.Model;

namespace MilkChess.UserInterface;

public class MovementIndicator
{
    private IChessBoard ChessBoard { get; set; }

    private readonly CursorSelector From;
    private readonly CursorSelector To;

    public Movement Movement { get; set; }

    public MovementIndicator(IChessBoard chessBoard)
    {
        this.ChessBoard = chessBoard;
        this.From = new CursorSelector(chessBoard, null, UIConstants.BaseMovementCursorResource);
        this.To = new CursorSelector(chessBoard, null, UIConstants.BaseMovementCursorResource);
    }

    public void AddToAbsoluteLayout(AbsoluteLayout layout)
    {
        layout.Add(From);
        layout.Add(To);
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
            this.ClearMovement();
        }
    }

    public void ClearMovement()
    {
        From.Position = null;
        To.Position = null;
        this.Movement = null;
    }

    public void Refresh()
    {
        From.Refresh();
        To.Refresh();
    }
}