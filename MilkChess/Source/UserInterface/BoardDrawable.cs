using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics.Platform;

namespace MilkChess.UserInterface;

using Microsoft.Maui.Graphics;

public class BoardDrawable : IDrawable
{
    private ILogger<BoardDrawable> Logger;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // var board = this.InternalLoadImage("board.png");
        // canvas.SetFillImage(board);
        
        LinearGradientPaint linearGradientPaint = new LinearGradientPaint
        {
            StartColor = Colors.Yellow,
            EndColor = Colors.Green,
            // StartPoint is already (0,0)
            EndPoint = new Point(1, 0)
        };

        RectF linearRectangle = new RectF(10, 10, 200, 100);
        canvas.SetFillPaint(linearGradientPaint, linearRectangle);
        canvas.SetShadow(new SizeF(10, 10), 10, Colors.Grey);
        canvas.FillRoundedRectangle(linearRectangle, 12);
    }

    private IImage InternalLoadImage(string imagePath)
    {
        IImage image;
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        using (Stream stream = assembly.GetManifestResourceStream("MilkChess.Resources.Images.large.board.png"))
        {
            // PlatformImage isn't currently supported on Windows.
            image = PlatformImage.FromStream(stream);
        }
        Console.WriteLine("load image {}", image);
        
        if (image == null)
        {
            Console.WriteLine("load image {} errro", image);

            throw new Exception("123");
        }
        
        return image;
    }
}