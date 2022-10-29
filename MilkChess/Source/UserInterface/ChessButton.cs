using MilkChess.Source.Model;
using Color = Microsoft.Maui.Graphics.Color;

namespace MilkChess.UserInterface;

public class ChessButton : ImageButton
{
    private readonly Position Position;

    public Chess Chess { get; set; }

    public ChessButton(Chess chess, Position position)
    {
        this.Position = position;
        this.Chess = chess;

        if (chess == null)
        {
            this.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        }
        else
        {
            string color = chess.Color == MilkChess.Source.Model.Color.RED ? "r" : "b";
            string symbol = chess.Symbol.ToLower();
            this.Source = $"{color}{symbol}.png";
        }
    }
}