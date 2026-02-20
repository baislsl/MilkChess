using Microsoft.Maui.Layouts;
using MilkChess.Model;

namespace MilkChess.UserInterface;

public class CursorSelector : Image
{
    private IChessBoard ChessBoard { get; set; }

    private Position _position;

    public Position Position
    {
        get => _position;

        set
        {
            this._position = value;
            Refresh();
        }
    }

    public Chess Chess => this.ChessBoard.GetChessAtPosition(this.Position);

    public CursorSelector(IChessBoard chessBoard, Position position, string source)
    {
        this.ChessBoard = chessBoard;
        this.Source = source;
        this.Position = position;
    }

    public void Refresh()
    {
        this.IsVisible = (this.Position != null);

        if (this.Position != null)
        {
            AbsoluteLayout.SetLayoutBounds(this, new Rect(
                _position.GetAbsoluteCoordinateX(),
                _position.GetAbsoluteCoordinateY(),
                AbsoluteLayout.AutoSize,
                AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
        }
    }
}