using Microsoft.Maui.Layouts;
using MilkChess.Model;
using Color = Microsoft.Maui.Graphics.Color;

namespace MilkChess.UserInterface;

public class ChessButton : ImageButton
{
    private Chess _chess;

    public readonly Position Position;

    public Chess Chess
    {
        get => _chess;

        set
        {
            this._chess = value;
            this.Source = value?.DrawableResource();
        }
    }

    public ChessButton(Chess chess, Position position)
    {
        this.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        this.Position = position;
        this.Chess = chess;
        this.ZIndex = UIConstants.ChessZIndex;

        Refresh();
    }

    public void Refresh()
    {
        this.Source = _chess?.DrawableResource();
        AbsoluteLayout.SetLayoutBounds(this, new Rect(
            Position.GetAbsoluteCoordinateX(),
            Position.GetAbsoluteCoordinateY(),
            UIConstants.ChessSize,
            UIConstants.ChessSize));
        AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
    }
}