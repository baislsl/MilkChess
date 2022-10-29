using Microsoft.Maui.Layouts;
using MilkChess.Source.Model;
using Color = Microsoft.Maui.Graphics.Color;

namespace MilkChess.UserInterface;

public class ChessButton : ImageButton
{
    private Chess _chess;

    private ChessModel ChessModelBinder;
    
    public readonly Position Position;

    public Chess Chess
    {
        get => _chess;

        set
        {
            this._chess = value;
            this.ChessModelBinder.SetChessAtPosition(value, this.Position, out _);
            this.Source = value?.DrawableResource();
        }
    }

    public ChessButton(ChessModel chessModel, Chess chess, Position position)
    {
        this.ChessModelBinder = chessModel;

        this.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        this.Position = position;
        this.Chess = chess;
        
        AbsoluteLayout.SetLayoutBounds(this, new Rect(
            position.GetAbsoluteCoordinateX(),
            position.GetAbsoluteCoordinateY(),
            UIConstants.ChessSize,
            UIConstants.ChessSize));
        AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
        
    }
}