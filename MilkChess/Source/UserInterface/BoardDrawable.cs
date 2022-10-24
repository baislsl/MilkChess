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
        var image = this.InternalLoadImage("MilkChess.Resources.Images.large.board.png");
        canvas.DrawImage(image, 0, 0, image.Width, image.Height);
        
        image = this.InternalLoadImage("MilkChess.Resources.Images.large.bb.png");
        canvas.DrawImage(image, 50, 50, image.Width, image.Height);
        
        
        
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