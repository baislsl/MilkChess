using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Protocol;

namespace MilkChess.UserInterface;

// TODO: merge PredictTracePreviewViewModel to ChessBoardViewModel?
public partial class PredictTracePreviewViewModel : ObservableObject, IChessBoardContext
{
    private readonly ILogger<PredictTracePreviewViewModel> Logger;

    private readonly ChessModel ChessModel;

    private readonly List<List<ChessButton>> Matrix;

    public HistoryManager HistoryManager { get; }

    public Rect ExecutionTurnLayoutBound => new Rect(UIConstants.HorizontalOffset / 2.0,
        UIConstants.ChessBoardHeight / 2.0,
        AbsoluteLayout.AutoSize,
        AbsoluteLayout.AutoSize);

    public GlobalConfig GlobalConfig { get; }

    public PredictTracePreviewViewModel(
        ILogger<PredictTracePreviewViewModel> logger,
        ChessModel chessModel,
        GlobalConfig globalConfig,
        HistoryManager historyManager)
    {
        this.Logger = logger;
        this.ChessModel = chessModel;
        this.GlobalConfig = globalConfig;
        this.HistoryManager = historyManager;

        Matrix = new List<List<ChessButton>>(Constants.ChessBoardWidth);
        for (int i = 0; i < Constants.ChessBoardHeight; i++)
        {
            Matrix.Add(new List<ChessButton>(new ChessButton[Constants.ChessBoardWidth]));
        }

        Iterator.IterateOverChessBoard(position =>
        {
            ChessButton chessButton = new ChessButton(null, position);
            Matrix[position.Y][position.X] = chessButton;
        });
    }

    public void InitFromPredictTrace(PredictMovementTrace trace)
    {
        var feedback = trace.RawInfoFeedback;
        this.AdoptForsythEdwardsNotation(feedback.InitFen);
        foreach (var movement in feedback.Movements)
        {
            this.HistoryManager.InsertMovement(movement.Clone());
        }

        this.MoveToStart();
    }


    public void AddToAbsoluteLayout(AbsoluteLayout layout)
    {
        Iterator.IterateOverChessBoard(position => { layout.Add(Matrix[position.Y][position.X]); });
    }

    [RelayCommand]
    private void MoveForward()
    {
        Movement movement = HistoryManager.MoveForward();
        if (movement != null)
        {
            this.ApplyMovement(movement);
        }
    }

    [RelayCommand]
    private void MoveBackward()
    {
        Movement movement = HistoryManager.MoveBackward();
        if (movement != null)
        {
            this.RevertMovement(movement);
        }
    }

    [RelayCommand]
    private void MoveToStart()
    {
        while (HistoryManager.MoveBackward() is { } movement)
        {
            this.RevertMovement(movement);
        }
    }

    [RelayCommand]
    private void MoveToEnd()
    {
        while (HistoryManager.MoveForward() is { } movement)
        {
            this.ApplyMovement(movement);
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private void SwitchToSelectedMovement(Movement movement)
    {
        Logger.LogInformation("[SwitchToSelectedMovement] {movement}", movement);

        int cursor = movement.ActionCount;

        while (HistoryManager.CanMoveForward() && cursor > HistoryManager.Cursor)
        {
            this.MoveForward();
        }

        while (HistoryManager.CanMoveBackward() && cursor < HistoryManager.Cursor)
        {
            this.MoveBackward();
        }
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
        SetChessAtPosition(null, movement.From, out _);
        SetChessAtPosition(movement.MovedChess, movement.To, out _);
    }

    public void RevertMovement(Movement movement)
    {
        SetChessAtPosition(movement.MovedChess, movement.From, out _);
        SetChessAtPosition(movement.RemovedChess, movement.To, out _);
    }

    public void MoveChess(Position from, Position to, out Chess removedChess)
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        ChessModel.Reset();
        HistoryManager.Reset();

        Iterator.IterateOverChessBoard(position =>
        {
            this.Matrix[position.Y][position.X].Chess = this.GetChessAtPosition(position);
        });
    }
}