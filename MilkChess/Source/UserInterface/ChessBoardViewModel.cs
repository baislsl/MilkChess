using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Source.Model;

namespace MilkChess.UserInterface;

public class ChessBoardViewModel
{
    public int ChessBoardHeight => UIConstants.ChessBoardHeight;
    public int ChessBoardWidth => UIConstants.ChessBoardWidth;

    public readonly List<List<ChessButton>> Martix;

    public readonly CursorSelector CursorSelector;

    public ChessButton SelectedChess = null;

    public readonly ChessModel ChessModel;

    public ILogger Logger;

    public ChessBoardViewModel(ILogger<ChessBoardViewModel> logger, ChessModel chessModel)
    {
        ChessModel = chessModel;
        Logger = logger;

        Martix = new List<List<ChessButton>>(ChessBoardHeight);
        for (int i = 0; i < ChessBoardHeight; i++)
        {
            Martix.Add(new List<ChessButton>(new ChessButton[ChessBoardWidth]));
        }

        this.CursorSelector = new CursorSelector(this.ChessModel, this.ChessModel.Cursor.Position);
        
        Logger.LogInformation("Init view model");
    }

    public EventHandler CreateOnClickedCommand(Position position)
    {
        return (sender, args) =>
        {
            ChessButton chessButton = (ChessButton) sender;

            if (chessButton == null) throw new ArgumentNullException("Chess button null");

            if (SelectedChess?.Chess == null)
            {
                SelectedChess = chessButton;
                CursorSelector.Position = chessButton.Position;
            }
            else
            {
                Chess moveChess = SelectedChess.Chess;
                SelectedChess.Chess = null;
                SelectedChess = null;
                chessButton.Chess = moveChess;
                CursorSelector.Position = chessButton.Position;
            }
        };
    }
}