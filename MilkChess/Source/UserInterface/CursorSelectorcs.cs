using Microsoft.Maui.Layouts;
using MilkChess.Source.Model;

namespace MilkChess.UserInterface;

public class CursorSelector : Image
{
    private ChessModel ChessModelBinder;

    private Position _position;

    public Position Position
    {
        get => _position;

        set
        {
            this._position = value;
            this.ChessModelBinder.Cursor.Position = value;

            AbsoluteLayout.SetLayoutBounds(this, new Rect(
                value.GetAbsoluteCoordinateX(),
                value.GetAbsoluteCoordinateY(),
                AbsoluteLayout.AutoSize,
                AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
        }
    }

    public CursorSelector(ChessModel chessModel, Position position)
    {
        this.ChessModelBinder = chessModel;

        this.Source = UIConstants.CursorBackgroundResource;
        this.Position = position;
    }
}