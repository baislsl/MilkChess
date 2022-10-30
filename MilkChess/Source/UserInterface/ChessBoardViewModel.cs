using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Ucci;

namespace MilkChess.UserInterface;

public class ChessBoardViewModel : IChessBoardHandler
{
    public int ChessBoardHeight => UIConstants.ChessBoardHeight;
    public int ChessBoardWidth => UIConstants.ChessBoardWidth;

    public readonly List<List<ChessButton>> Martix;

    public readonly CursorSelector CursorSelector;

    public ChessButton SelectedChess = null;

    public readonly ChessModel ChessModel;

    public readonly ILogger Logger;

    public HistoryManager HistoryManager { get; }

    public ChessBoardViewModel(ILogger<ChessBoardViewModel> logger, ChessModel chessModel,
                    HistoryManager historyManager)
    {
        Logger = logger;
        ChessModel = chessModel;
        HistoryManager = historyManager;

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
        Logger.LogInformation(
                        $"Clicked {clickedButton.Position} {clickedButton.Chess}; Selected {SelectedChess?.Position}, {SelectedChess?.Chess}");

        if (SelectedChess?.Chess == null
            || SelectedChess.Chess.Color != HistoryManager.NextColorStep()
            || !ChessModel.ValidateMovement(SelectedChess.Position, clickedButton.Position))
        {
            SelectedChess = clickedButton;
            CursorSelector.Position = clickedButton.Position;

            Logger.LogInformation($"MoveSelector {SelectedChess.Chess} {CursorSelector.Position}");
        }
        else
        {
            Movement movement = new Movement
            {
                            MovedChess = SelectedChess.Chess,
                            RemovedChess = clickedButton.Chess,
                            From = SelectedChess.Position,
                            To = clickedButton.Position
            };
            movement.GetStandardDescription();  // trigger cachin StandardDescription

            Logger.LogInformation(
                            $"MoveChess {SelectedChess.Chess} {SelectedChess.Position}->{clickedButton.Position}");
            Logger.LogInformation($"{movement.ToChineseViewableString(this)}");

            this.ApplyMovement(movement);
            SelectedChess = null;
            CursorSelector.Position = clickedButton.Position;

            HistoryManager.InsertMovement(movement);

            // // Move from SelectedChess to chessButton
            // Chess moveChess = SelectedChess.Chess;
            // SelectedChess.Chess = null;
            // SelectedChess = null;
            // clickedButton.Chess = moveChess;
            // CursorSelector.Position = clickedButton.Position;
        }
    }

    public void Reset()
    {
        ChessModel.Reset();
        HistoryManager.Reset();

        Iterator.IterateOverChessBoard(position =>
        {
            this.Martix[position.Y][position.X].Chess = ChessModel.GetChessAtPosition(position);
        });

        this.SelectedChess = null;
    }

    public bool MoveForward()
    {
        Movement movement = HistoryManager.MoveForward();
        if (movement != null)
        {
            this.ApplyMovement(movement);
        }

        return movement != null;
    }

    public bool MoveBackward()
    {
        Movement movement = HistoryManager.MoveBackward();
        if (movement != null)
        {
            this.RevertMovement(movement);
        }

        return movement != null;
    }

    public void ApplyMovement(Movement movement)
    {
        GetChessButtonAt(movement.From).Chess = null;
        GetChessButtonAt(movement.To).Chess = movement.MovedChess;
    }

    public void RevertMovement(Movement movement)
    {
        GetChessButtonAt(movement.From).Chess = movement.MovedChess;
        GetChessButtonAt(movement.To).Chess = movement.RemovedChess;
    }

    private ChessButton GetChessButtonAt(Position position)
    {
        return this.Martix[position.Y][position.X];
    }


    public Chess GetChessAtPosition(Position position)
    {
        return this.Martix[position.Y][position.X].Chess;
    }

    public void SetChessAtPosition(Chess chess, Position position, out Chess removedChess)
    {
        removedChess = this.GetChessAtPosition(position);
        GetChessButtonAt(position).Chess = chess;
    }
}