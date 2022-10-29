using Microsoft.Maui.Layouts;
using MilkChess.Source.Helper;
using MilkChess.Source.Model;
using MilkChess.Ucci;
using Color = Microsoft.Maui.Graphics.Color;
using static MilkChess.UserInterface.UIConstants;

namespace MilkChess.UserInterface;

public class ChessBoardLayout : AbsoluteLayout
{
    public static readonly BindableProperty ChessModelProperty =
        BindableProperty.Create("ChessModel", typeof(ChessModel), typeof(ChessBoardLayout));

    private List<List<ChessButton>> Martix;

    private CursorSelector CursorSelector;

    private ChessButton SelectedChess = null;

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
        this.ChessModel ??= ChessModel.Instance;
        this.Martix = new List<List<ChessButton>>(Constants.ChessBoardHeight);
        for (int i = 0; i < Constants.ChessBoardHeight; i++)
        {
            this.Martix.Add(new List<ChessButton>(new ChessButton[Constants.ChessBoardWidth]));
        }

        this.buildBackground()
            .buildCursor()
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
            ChessButton chessButton = new ChessButton(this.ChessModel, chess, position);

            chessButton.Clicked += this.CreateOnClickedCommand(position);
            this.Children.Add(chessButton);
            this.Martix[position.Y][position.X] = chessButton;
        });

        return this;
    }

    public EventHandler CreateOnClickedCommand(Position position)
    {
        return (sender, args) =>
        {
            ChessButton chessButton = (ChessButton) sender;

            if (chessButton == null) throw new ArgumentNullException("Chess button null");
            
            if (this.SelectedChess?.Chess == null)
            {
                this.SelectedChess = chessButton;
                this.CursorSelector.Position = chessButton.Position;
            }
            else
            {
                Chess moveChess = this.SelectedChess.Chess;
                this.SelectedChess.Chess = null;
                this.SelectedChess = null;
                chessButton.Chess = moveChess;
                this.CursorSelector.Position = chessButton.Position;
            }
        };
    }

    public ChessBoardLayout buildCursor()
    {
        this.CursorSelector = new CursorSelector(this.ChessModel, this.ChessModel.Cursor.Position);
        this.Children.Add(CursorSelector);
        return this;
    }
}
