using Microsoft.Maui.Layouts;
using MilkChess.Model;

namespace MilkChess.UserInterface;

public class CursorSelector : Image
{
    private Position _position;

    public Position Position
    {
        get => _position;

        set
        {
            this._position = value;

            AbsoluteLayout.SetLayoutBounds(this, new Rect(
                value.GetAbsoluteCoordinateX(),
                value.GetAbsoluteCoordinateY(),
                AbsoluteLayout.AutoSize,
                AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
        }
    }

    public CursorSelector(Position position, string source)
    {
        this.Source = source;
        this.Position = position;
    }
}