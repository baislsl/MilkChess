using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Ucci;
using Color = MilkChess.Model.Color;

namespace MilkChess.UserInterface;

public partial class EditChessBoardViewModel : ObservableObject, IChessBoard
{
    private readonly ILogger<EditChessBoardViewModel> Logger;

    private readonly ChessModel ChessModel;

    private readonly List<List<ChessButton>> Matrix;

    [ObservableProperty] private GlobalConfig _globalConfig;

    [ObservableProperty] private IdleChessPanel _idleChessPanel;

    [ObservableProperty] private CursorSelector _chessBoardCursor;

    public EditChessBoardViewModel(
        ILogger<EditChessBoardViewModel> logger,
        IdleChessPanel idleChessPanel,
        ChessModel chessModel,
        GlobalConfig globalConfig)
    {
        Logger = logger;
        ChessModel = chessModel;
        IdleChessPanel = idleChessPanel;
        GlobalConfig = globalConfig;

        Matrix = new List<List<ChessButton>>(Constants.ChessBoardWidth);
        for (int i = 0; i < Constants.ChessBoardHeight; i++)
        {
            Matrix.Add(new List<ChessButton>(new ChessButton[Constants.ChessBoardWidth]));
        }

        Iterator.IterateOverChessBoard(position =>
        {
            ChessButton chessButton = new ChessButton(null, position);

            // chessButton.Released += this.WrapEventHandler(this.OnChessSelected);
            chessButton.Command = ChessClickedCommand;
            chessButton.RightClickCommand = ChessRightClickedCommand;
            chessButton.CommandParameter = chessButton;

            Matrix[position.Y][position.X] = chessButton;
        });

        ChessBoardCursor = new CursorSelector(this, null);
    }

    public void LoadChessBoardLayout(IChessBoard chessBoard)
    {
        IdleChessPanel.Reset();

        Iterator.IterateOverChessBoard(position =>
        {
            var chess = chessBoard.GetChessAtPosition(position);
            this.SetChessAtPosition(chess, position, out _);

            if (chess != null)
            {
                IdleChessPanel.RemoveChess(chess);
            }
        });

        this.ResetCursor();
    }

    public void AddToAbsoluteLayout(AbsoluteLayout layout)
    {
        Iterator.IterateOverChessBoard(position => { layout.Add(Matrix[position.Y][position.X]); });
    }

    public int ChessBoardHeight => UIConstants.ChessBoardHeight;

    public int ChessBoardWidth => UIConstants.ChessBoardWidth;

    public void Reset()
    {
        ResetChessBoard();
    }

    private void ResetCursor()
    {
        ChessBoardCursor.Position = null;
        IdleChessPanel.ResetCursor();
    }

    [RelayCommand]
    private void ResetChessBoard()
    {
        ChessModel.Reset();

        Iterator.IterateOverChessBoard(position =>
        {
            this.SetChessAtPosition(ChessModel.GetChessAtPosition(position), position, out _);
        });

        this.IdleChessPanel.Clear();
        this.ResetCursor();
    }

    [RelayCommand]
    private void ClearChessBoard()
    {
        Iterator.IterateOverChessBoard(position => { this.SetChessAtPosition(null, position, out _); });

        this.IdleChessPanel.Reset();
        this.ResetCursor();

        // Set King back.
        this.SetChessAtPosition(new King(Color.RED), new Position(4, 0), out _);
        this.SetChessAtPosition(new King(Color.BLACK), new Position(4, 9), out _);
        this.IdleChessPanel.RemoveChess(new King(Color.RED));
        this.IdleChessPanel.RemoveChess(new King(Color.BLACK));
    }

    public Chess GetChessAtPosition(Position position)
    {
        return ChessModel.GetChessAtPosition(position);
    }

    public void SetChessAtPosition(Chess chess, Position position, out Chess removedChess)
    {
        ChessModel.SetChessAtPosition(chess, position, out removedChess);
        this.Matrix[position.Y][position.X].Chess = chess;
    }

    public void ApplyMovement(Movement movement)
    {
        throw new NotImplementedException();
    }

    public void RevertMovement(Movement movement)
    {
        throw new NotImplementedException();
    }

    public void MoveChess(Position from, Position to, out Chess removedChess)
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void OnChessRightClicked(ChessButton clickedButton)
    {
        Logger.LogInformation("Right clicked {} {}", clickedButton.Position, clickedButton.Chess);
        if (clickedButton.Chess != null)
        {
            // move chess
            IdleChessPanel.InsertChess(clickedButton.Chess);
            SetChessAtPosition(null, clickedButton.Position, out _);

            this.ResetCursor();
        }
    }

    [RelayCommand]
    private void OnChessClicked(ChessButton clickedButton)
    {
        Logger.LogInformation("Clicked {} {}", clickedButton.Position, clickedButton.Chess);

        if (clickedButton.Chess == null)
        {
            if (ChessBoardCursor.Chess != null && ChessBoardCursor.Chess.IsValidPosition(clickedButton.Position))
            {
                // Move chess
                this.SetChessAtPosition(ChessBoardCursor.Chess, clickedButton.Position, out _);
                this.SetChessAtPosition(null, ChessBoardCursor.Position, out _);
                this.ResetCursor();
            }
            else if (IdleChessPanel.Cursor is { IsIdle: true } &&
                     IdleChessPanel.Cursor.Chess.IsValidPosition(clickedButton.Position))
            {
                this.SetChessAtPosition(IdleChessPanel.Cursor.Chess, clickedButton.Position, out _);
                IdleChessPanel.Cursor.IsIdle = false;
                this.ResetCursor();
            }
            else
            {
                // Set cursor to new location
                ChessBoardCursor.Position = clickedButton.Position;
            }
        }
        else
        {
            ChessBoardCursor.Position = clickedButton.Position;
            IdleChessPanel.ResetCursor();
        }
    }

    [RelayCommand]
    private void OnIdleChessClicked(IdleChess chess)
    {
        Logger.LogInformation("OnIdleChessClicked {}", chess);

        if (chess.IsIdle)
        {
            // set cursor
            IdleChessPanel.Cursor = chess;
            ChessBoardCursor.Position = null;
        }
        else
        {
            if (ChessBoardCursor.Chess != null && ChessBoardCursor.Chess is not King)
            {
                // move chess
                IdleChessPanel.InsertChess(ChessBoardCursor.Chess);
                SetChessAtPosition(null, ChessBoardCursor.Position, out _);

                this.ResetCursor();
            }
            else
            {
                this.ResetCursor();
            }
        }
    }

    [RelayCommand]
    private void OnRotateChessBoard()
    {
        GlobalConfig.IsRotated = !GlobalConfig.IsRotated;

        Iterator.IterateOverChessBoard(position => { Matrix[position.Y][position.X].Refresh(); });

        this.ChessBoardCursor.Refresh();
    }
}