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

    public readonly ILogger Logger;

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

    public void OnChessSelected(object sender, EventArgs args)
    {
        ChessButton clickedButton = (ChessButton) sender;
        Logger.LogInformation($"Clicked {clickedButton.Position} {clickedButton.Chess}; Selected {SelectedChess?.Position}, {SelectedChess?.Chess}");

        // var v = ChessModel.ValidateMovement(SelectedChess?.Position, chessButton.Position);
        if (SelectedChess?.Chess == null || !ChessModel.ValidateMovement(SelectedChess.Position, clickedButton.Position))
        {
            SelectedChess = clickedButton;
            CursorSelector.Position = clickedButton.Position;
            
            Logger.LogInformation($"MoveSelector {SelectedChess.Chess} {CursorSelector.Position}");

        }
        else
        {
            Logger.LogInformation("MoveChess ");
            // Move from SelectedChess to chessButton
            Chess moveChess = SelectedChess.Chess;
            SelectedChess.Chess = null;
            SelectedChess = null;
            clickedButton.Chess = moveChess;
            CursorSelector.Position = clickedButton.Position;
        }
    }

    public void Reset()
    {
        ChessModel.Reset();
        Iterator.IterateOverChessBoard(position =>
        {
            this.Martix[position.Y][position.X].Chess = ChessModel.GetChessAtPosition(position);
            ;
        });

        this.SelectedChess = null;
    }
}