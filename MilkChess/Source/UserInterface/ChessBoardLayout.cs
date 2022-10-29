using Microsoft.Maui.Layouts;
using MilkChess.Source.Helper;
using MilkChess.Source.Model;
using Color = Microsoft.Maui.Graphics.Color;
using static MilkChess.UserInterface.UIConstants;

namespace MilkChess.UserInterface;

public class ChessBoardLayout : AbsoluteLayout
{
    public static readonly BindableProperty ChessModelProperty =
        BindableProperty.Create("ChessModel", typeof(ChessModel), typeof(ChessBoardLayout));

    public ChessModel ChessModel
    {
        get => (ChessModel) GetValue(ChessModelProperty);
        set => SetValue(ChessModelProperty, value);
    }

    // public ChessModel ChessModel { get; set; }

    public ChessBoardLayout()
    {
        this.WidthRequest = UIConstants.ChessBoardWidth;
        this.HeightRequest = UIConstants.ChessBoardHeight;
        
        // TODO: remove hard static object
        this.ChessModel = ChessModel.Instance;

        this.buildBackground()
            .buildChessItems();
    }

    public ChessBoardLayout buildBackground()
    {
        // return this;
        Image backgroundImage = new Image()
        {
            Source = UIConstants.ChessBoardBackgroundResource
        };
        AbsoluteLayout.SetLayoutBounds(backgroundImage,
            new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(backgroundImage, AbsoluteLayoutFlags.PositionProportional);
        this.Children.Add(backgroundImage);
        return this;
    }


    public ChessBoardLayout buildChessItems()
    {
        Iterator.IterateOverChessBoard(position =>
        {
            Chess chess = this.ChessModel.GetChessAtPosition(position);
            ChessButton chessButton = new ChessButton(chess, position);

            AbsoluteLayout.SetLayoutBounds(chessButton,
                new Rect(
                    position.X * ChessSize + HorizontalOffset,
                    //(9 - position.Y) * ChessSize + VerticalOffset,
                    (9 - position.Y) * ChessSize + VerticalOffset,
                    53, 53));
            AbsoluteLayout.SetLayoutFlags(chessButton, AbsoluteLayoutFlags.None);
            
            chessButton.Clicked += this.CreateOnClickedCommand(position);
            this.Children.Add(chessButton);
        });

        return this;
    }

    public EventHandler CreateOnClickedCommand(Position position)
    {
        return (sender, args) =>
        {
            // TODO:
        };
    }

    public ChessBoardLayout buildCursor()
    {
        return this;
    }
}