using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Protocol;
using Color = Microsoft.Maui.Graphics.Color;

namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel : ObservableObject, IChessBoardContext
{
    private readonly ChessModel ChessModel;

    private readonly List<List<ChessButton>> Matrix;

    private readonly ILogger Logger;

    private readonly IServiceProvider ServiceProvider;

    private readonly MilkChessConfig Config;

    private readonly MilkChessConfigService MilkChessConfigService;

    private readonly AlertService AlertService;

    [ObservableProperty] private GlobalConfig _globalConfig;

    [ObservableProperty] private CursorSelector _cursorSelector;

    [ObservableProperty] private MovementIndicator _movementIndicator;

    private static readonly Color ActiveInteractiveModeColor = Colors.Blue;

    private static readonly Color InactiveInteractiveModeColor = Colors.CornflowerBlue;

    public HistoryManager HistoryManager { get; }

    public ChessBoardViewModel(
        IServiceProvider serviceProvider,
        ILogger<ChessBoardViewModel> logger,
        HistoryManager historyManager,
        ChessModel chessModel,
        MilkChessConfig milkChessConfig,
        GlobalConfig globalConfig,
        EngineFeedback engineFeedback,
        MilkChessConfigService milkChessConfigService,
        AlertService alertService)
    {
        ChessModel = chessModel;
        Logger = logger;
        HistoryManager = historyManager;
        ServiceProvider = serviceProvider;
        Config = milkChessConfig;
        EngineFeedback = engineFeedback;
        GlobalConfig = globalConfig;
        MilkChessConfigService = milkChessConfigService;
        AlertService = alertService;

        Matrix = new List<List<ChessButton>>(Constants.ChessBoardWidth);
        for (int i = 0; i < Constants.ChessBoardHeight; i++)
        {
            Matrix.Add(new List<ChessButton>(new ChessButton[Constants.ChessBoardWidth]));
        }

        Iterator.IterateOverChessBoard(position =>
        {
            ChessButton chessButton = new ChessButton(null, position);

            chessButton.Command = this.ChessSelectedCommand;
            chessButton.CommandParameter = chessButton;

            Matrix[position.Y][position.X] = chessButton;
        });

        if (!LoadChessManual())
        {
            Logger.LogInformation("Use default chess position");
            // If init manual not loaded successfully, refresh with the default chess position
            Iterator.IterateOverChessBoard(position =>
            {
                this.SetChessAtPosition(ChessModel.GetChessAtPosition(position), position, out _);
            });
        }
        else
        {
            Logger.LogInformation("Use chess position from previous execution");
        }

        CursorSelector = new CursorSelector(this, null);
        MovementIndicator = new MovementIndicator(this);
    }

    public void AddToAbsoluteLayout(AbsoluteLayout layout)
    {
        Iterator.IterateOverChessBoard(position => { layout.Add(Matrix[position.Y][position.X]); });
    }

    // 不set AllowConcurrentExecutions会导致执行期间其他棋子变虚化
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task OnChessSelected(ChessButton clickedButton)
    {
        Logger.LogInformation(
            $"Clicked {clickedButton.Position} {clickedButton.Chess}; Selected {CursorSelector.Position}, {CursorSelector.Chess}");

        if (CursorSelector.Chess != null
            && CursorSelector.Chess.Color == HistoryManager.ExecutionTurn
            && ChessModel.ValidateMovement(CursorSelector.Position, clickedButton.Position))
        {
            var movement = this.CreateMovement(CursorSelector.Position, clickedButton.Position);

            Logger.LogInformation("Execute movement {}", movement);

            this.ApplyMovement(movement);
            HistoryManager.InsertMovement(movement);

            CursorSelector.Position = clickedButton.Position;

            await RefreshUciEngineAsync();
        }
        else
        {
            CursorSelector.Position = clickedButton.Position;

            Logger.LogInformation($"MoveSelector to {CursorSelector.Position}");
        }
    }

    public virtual void ApplyMovement(Movement movement)
    {
        SetChessAtPosition(null, movement.From, out _);
        SetChessAtPosition(movement.MovedChess, movement.To, out _);

        this.MovementIndicator.Reset();
    }

    public virtual Chess GetChessAtPosition(Position position)
    {
        return ChessModel.GetChessAtPosition(position);
    }

    public virtual void SetChessAtPosition(Chess chess, Position position, out Chess removedChess)
    {
        ChessModel.SetChessAtPosition(chess, position, out removedChess);
        this.Matrix[position.Y][position.X].Chess = chess;
    }

    public virtual void RevertMovement(Movement movement)
    {
        SetChessAtPosition(movement.MovedChess, movement.From, out _);
        SetChessAtPosition(movement.RemovedChess, movement.To, out _);

        this.MovementIndicator.Reset();
    }

    public virtual void MoveChess(Position from, Position to, out Chess removedChess)
    {
        Chess chess = this.GetChessAtPosition(from);
        this.SetChessAtPosition(chess, to, out removedChess);

        this.MovementIndicator.Reset();
    }

    public virtual void Reset()
    {
        ChessModel.Reset();
        HistoryManager.Reset();
        MovementIndicator.Reset();
        EngineFeedback.Clear();

        Iterator.IterateOverChessBoard(position =>
        {
            this.Matrix[position.Y][position.X].Chess = this.GetChessAtPosition(position);
        });

        this.CursorSelector.Position = null;
    }

    private async Task EditAsync()
    {
        var editChessBoardPage = ServiceProvider.GetRequiredService<EditChessBoardPage>();
        editChessBoardPage.EditChessBoardViewModel.LoadChessBoardLayout(this);

        if (Application.Current == null || Application.Current.MainPage == null)
        {
            Logger.LogError("Null application context");
            return;
        }

        await Application.Current.MainPage.Navigation.PushModalAsync(editChessBoardPage);

        var result = await editChessBoardPage.EditChessBoardResult.Task;

        if (result.IsConfirmed)
        {
            Logger.LogInformation("Loading editor chess board ");

            this.Reset();

            Iterator.IterateOverChessBoard(position => this.SetChessAtPosition(
                result.ChessBoard.GetChessAtPosition(position),
                position,
                out _
            ));
        }
        else
        {
            Logger.LogInformation("User cancelled editing chess board");
        }
    }

    private async Task ConfigureEngineAsync()
    {
        var configureEnginePage = ServiceProvider.GetRequiredService<EngineConfigPage>();

        if (Application.Current == null || Application.Current.MainPage == null)
        {
            Logger.LogError("Null application context");
            return;
        }

        await Application.Current.MainPage.Navigation.PushModalAsync(configureEnginePage);
    }

    private void MoveForward()
    {
        Movement movement = HistoryManager.MoveForward();
        if (movement != null)
        {
            this.ApplyMovement(movement);
        }
    }

    private void MoveBackward()
    {
        Movement movement = HistoryManager.MoveBackward();
        if (movement != null)
        {
            this.RevertMovement(movement);
        }
    }

    private void MoveToStart()
    {
        while (HistoryManager.MoveBackward() is { } movement)
        {
            this.RevertMovement(movement);
        }
    }

    private void MoveToEnd()
    {
        while (HistoryManager.MoveForward() is { } movement)
        {
            this.ApplyMovement(movement);
        }
    }

    private void SwitchInteractMode(EngineInteractiveMode mode)
    {
        if (this.InteractiveMode != mode)
        {
            Logger.LogInformation("Switch interactive mode {} -> {}", this.InteractiveMode, mode);

            this.InteractiveMode = mode;
        }
    }

    private void SaveChessManual()
    {
        Config.LastChessBoardFen = this.BuildForsythEdwardsNotation().GetFenString();
        MilkChessConfigService.Save(Config);
    }

    private bool LoadChessManual()
    {
        if (ForsythEdwardsNotation.ParseString(Config.LastChessBoardFen, out ForsythEdwardsNotation forsythEdwardsNotation))
        {
            this.AdoptForsythEdwardsNotation(forsythEdwardsNotation);
        }
        else
        {
            Logger.LogError("Invalid fen string {Config.LastFenChessBoard}", Config.LastChessBoardFen);
        }

        return false;
    }

    private void RotateBoard()
    {
        GlobalConfig.IsRotated = !GlobalConfig.IsRotated;

        Iterator.IterateOverChessBoard(position => { Matrix[position.Y][position.X].Refresh(); });

        this.CursorSelector.Refresh();
        this.MovementIndicator.Refresh();
    }

    private void ExchangeTurn()
    {
        this.HistoryManager.ExchangeTurn();
        this.MovementIndicator.Reset();
    }

    private void InvokeBestMovement()
    {
        var movement = this.MovementIndicator?.Movement;

        if (movement != null)
        {
            Logger.LogInformation("[InvokeBestMovement] {}", movement);

            var newMovement = this.CreateMovement(movement.From, movement.To);

            this.ApplyMovement(newMovement);
            HistoryManager.InsertMovement(newMovement);
        }
    }

    private void SwitchToSelectedMovement(object sender)
    {
        Logger.LogDebug("[SwitchToSelectedMovement] {}", sender);

        Movement senderMovement = (Movement)sender;
        int cursor = senderMovement.ActionCount;

        if (cursor < HistoryManager.LastMovement.ActionCount)
        {
            while (HistoryManager.LastMovement.ActionCount != cursor)
            {
                MoveBackward();
            }
        }
        else
        {
            while (HistoryManager.LastMovement.ActionCount != cursor)
            {
                MoveForward();
            }
        }
    }

    private bool IsEngineOperatingChessBoard()
    {
        switch (InteractiveMode)
        {
            case EngineInteractiveMode.None:
            case EngineInteractiveMode.AnalyzeMode:
            case EngineInteractiveMode.IdleMode:
                return false;
            case EngineInteractiveMode.AutoMode:
            case EngineInteractiveMode.BlackAutoMode:
            case EngineInteractiveMode.RedAutoMode:
                // TODO: If black/red auto, it doesn't operating when turn is red/black
                return true;
        }

        return false;
    }
}