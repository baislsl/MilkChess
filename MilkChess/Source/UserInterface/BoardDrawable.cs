using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics.Platform;
using MilkChess.Source.Helper;
using MilkChess.Source.Model;
using static MilkChess.Source.Model.Color;

namespace MilkChess.UserInterface;

using Microsoft.Maui.Graphics;

public class BoardDrawable : IDrawable
{
    private const int HorizontalOffset = 39;
    private const int VerticalOffset = 39;
    private const int ChessSize = 55;

    private ChessModel ChessModel;

    public BoardDrawable(ChessModel chessModel)
    {
        this.ChessModel = chessModel;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var image = this.InternalLoadImage("MilkChess.Resources.Images.large.board.png");
        canvas.DrawImage(image, 0, 0, image.Width, image.Height);

        Iterator.IterateOverChessBoard(position =>
        {
            var chess = ChessModel.GetChessAtPosition(position);
            if (chess != null)
            {
                var chessImage = this.LoadResourceImage(chess);
                canvas.DrawImage(
                    chessImage,
                    position.X * ChessSize + HorizontalOffset,
                    (9-position.Y) * ChessSize + VerticalOffset,
                    chessImage.Width,
                    chessImage.Height);
            }
        });
    }

    public IImage LoadResourceImage(Chess chess)
    {
        string color = chess.Color == RED ? "r" : "b";
        string symbol = chess.Symbol.ToLower();

        string imagePath = $"MilkChess.Resources.Images.large.{color}{symbol}.png";

        return this.InternalLoadImage(imagePath);
    }

    private IImage InternalLoadImage(string imagePath)
    {
        IImage image;
        Assembly assembly = GetType().GetTypeInfo().Assembly;

        using (Stream stream = assembly.GetManifestResourceStream(imagePath))
        {
            // PlatformImage isn't currently supported on Windows.
            image = PlatformImage.FromStream(stream);
        }

        return image;
    }
}