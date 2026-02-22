using MilkChess.Model;
using Color = MilkChess.Model.Color;

namespace MilkChess.UserInterface;

public class MovementIndicator
{
    private IChessBoard ChessBoard { get; set; }

    private readonly CursorSelector From;
    private readonly CursorSelector To;

    public Movement Movement { get; private set; }

    public MovementIndicator(IChessBoard chessBoard)
    {
        this.ChessBoard = chessBoard;
        this.From = new CursorSelector(chessBoard, null, UIConstants.BlackMovementCursorResource);
        this.To = new CursorSelector(chessBoard, null, UIConstants.BlackMovementCursorResource);
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
            var uiSource = movement.ExecutedColor == Color.BLACK
                ? UIConstants.BlackMovementCursorResource
                : UIConstants.RedMovementCursorResource;

            From.Position = movement.From;
            To.Position = movement.To;
            From.Source = To.Source = uiSource;
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
        From.Refresh();
        To.Refresh();
    }
}