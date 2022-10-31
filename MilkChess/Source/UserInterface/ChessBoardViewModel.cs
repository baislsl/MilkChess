using System.Windows.Input;
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
    }

    public void OnChessSelected(object sender, EventArgs args)
    {
        ChessButton clickedButton = (ChessButton) sender;
        Logger.LogInformation(
                        $"Clicked {clickedButton.Position} {clickedButton.Chess}; Selected {SelectedChess?.Position}, {SelectedChess?.Chess}");

        if (SelectedChess?.Chess == null
            || SelectedChess.Chess.Color != HistoryManager.NextColorStep
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
            movement.GetStandardDescription(); // trigger cachin StandardDescription

            Logger.LogInformation(
                            $"MoveChess {SelectedChess.Chess} {SelectedChess.Position}->{clickedButton.Position}");
            Logger.LogInformation($"{movement.ToChineseViewableString(this)}");

            this.ApplyMovement(movement);
            SelectedChess = null;
            CursorSelector.Position = clickedButton.Position;

            HistoryManager.InsertMovement(movement);

            Logger.LogInformation($"fen {this.BuildForsythEdwardsNotation().GetFenString()}");
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

    public void MoveForward()
    {
        Movement movement = HistoryManager.MoveForward();
        if (movement != null)
        {
            this.ApplyMovement(movement);
        }
    }

    public void MoveBackward()
    {
        Movement movement = HistoryManager.MoveBackward();
        if (movement != null)
        {
            this.RevertMovement(movement);
        }
    }

    public void MoveToStart()
    {
    }

    public void MoveToEnd()
    {
        var ret = this.AdoptForsythEdwardsNotation(
                        "rnbakabnr/9/4c4/p1p1p1p1p/9/9/P1P1P1P1P/1C3CN2/9/1RBAKAB1R b - - 0 1");
        Logger.LogInformation($"adopt {ret}");
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

    public ICommand ResetCommand => this.WrapClickCommand(this.Reset, "Reset");

    public ICommand MoveBackwardCommand => this.WrapClickCommand(this.MoveBackward, "MoveBackward");

    public ICommand MoveForwardCommand => this.WrapClickCommand(this.MoveForward, "MoveForward");

    public ICommand MoveToStartCommand => this.WrapClickCommand(this.MoveToStart, "MoveToStart");

    public ICommand MoveToEndCommand => this.WrapClickCommand(this.MoveToEnd, "MoveToEnd");

    private ICommand WrapClickCommand(Action action, string commandName)
    {
        return new Command<object>((object Object) =>
        {
            try
            {
                Logger.LogInformation($"Received command {commandName}");
                action();
                Logger.LogInformation($"Finished command {commandName}");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "MilkChess click Event error");
            }
        }, canExecute: (object o) => true);
    }
}