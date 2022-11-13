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
            Refresh();
        }
    }

    public CursorSelector(Position position, string source)
    {
        this.Source = source;
        this.Position = position;
    }

    public void Refresh()
    {
        AbsoluteLayout.SetLayoutBounds(this, new Rect(
                        _position.GetAbsoluteCoordinateX(),
                        _position.GetAbsoluteCoordinateY(),
                        AbsoluteLayout.AutoSize,
                        AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
    }
}