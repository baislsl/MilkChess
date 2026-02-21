using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Helper;
using MilkChess.Model;
using Color = Microsoft.Maui.Graphics.Color;


namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel
{
    public string ConnectOrDisconnectEngineText =>
        this.Engine == null ? UIConstants.ConnectEngineText : UIConstants.DisconnectEngineText;

    public Color ScoreColor => this.Score > 0 ? Colors.Red : Colors.Black;

    [ObservableProperty] private int _score;

    [ObservableProperty] private EngineInteractiveMode interactiveMode = EngineInteractiveMode.AnalyzeMode;

    private bool _IsEngineAutoMoving;

    private bool IsEngineAutoMoving
    {
        get => _IsEngineAutoMoving;
        set
        {
            if (value != _IsEngineAutoMoving)
            {
                _IsEngineAutoMoving = value;
                // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResetCommand)));
                // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveBackwardCommand)));
                // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveForwardCommand)));
                // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveToStartCommand)));
                // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveToEndCommand)));
                // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResetCommand)));
            }
        }
    }

    public ObservableCollection<PredictMovementTrace> EngineSuggestedMovements { get; set; }

    public int ChessBoardHeight => UIConstants.ChessBoardHeight;

    public int ChessBoardWidth => UIConstants.ChessBoardWidth;

    public ObservableCollection<Movement> HistoryMovements => (ObservableCollection<Movement>)HistoryManager.Movements;

    public Color SwitchToStopModeColor => (this.InteractiveMode == EngineInteractiveMode.StopMode)
        ? ActiveInteractiveModeColor
        : InactiveInteractiveModeColor;

    public Color SwitchToAnalyzeModeColor => (this.InteractiveMode == EngineInteractiveMode.AnalyzeMode)
        ? ActiveInteractiveModeColor
        : InactiveInteractiveModeColor;

    public Color SwitchToBlackAutoModeColor => (this.InteractiveMode == EngineInteractiveMode.BlackAutoMode)
        ? ActiveInteractiveModeColor
        : InactiveInteractiveModeColor;

    public Color SwitchToRedAutoModeColor => (this.InteractiveMode == EngineInteractiveMode.RedAutoMode)
        ? ActiveInteractiveModeColor
        : InactiveInteractiveModeColor;

    public Color SwitchToAutoModeColor => (this.InteractiveMode == EngineInteractiveMode.AutoMode)
        ? ActiveInteractiveModeColor
        : InactiveInteractiveModeColor;

    // Compiler will throw error result if I remove this unused fields.
    // This seems to be the compiler bug.
    public string ChineseViewableString => default;
    public string ActionCount => default;
    public string Movements => default;
    public string Depth => default;

    #region command definition

    public ICommand ResetCommand =>
        CommandMiddlewareHelper.BuildCommand(
            new Command(this.Reset, () => !IsEngineAutoMoving),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand MoveBackwardCommand =>
        CommandMiddlewareHelper.BuildCommand(
            new Command(this.MoveBackward, () => !IsEngineAutoMoving),
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand MoveForwardCommand =>
        CommandMiddlewareHelper.BuildCommand(
            new Command(this.MoveForward, () => !IsEngineAutoMoving),
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand MoveToStartCommand =>
        CommandMiddlewareHelper.BuildCommand(
            new Command(this.MoveToStart, () => !IsEngineAutoMoving),
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand MoveToEndCommand =>
        CommandMiddlewareHelper.BuildCommand(
            new Command(this.MoveToEnd, () => !IsEngineAutoMoving),
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand ConnectOrDisconnectEngineCommand =>
        CommandMiddlewareHelper.BuildCommand(
            this.ConnectOrDisconnectEngine,
            this.CommonCommandMiddleware());

    public ICommand StopEngineCommand =>
        CommandMiddlewareHelper.BuildCommand(
            () => this.SwitchInteractMode(EngineInteractiveMode.StopMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand SwitchToAnalyzeModeCommand =>
        CommandMiddlewareHelper.BuildCommand(
            () => this.SwitchInteractMode(EngineInteractiveMode.AnalyzeMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand SwitchToBlackAutoModeCommand =>
        CommandMiddlewareHelper.BuildCommand(
            () => this.SwitchInteractMode(EngineInteractiveMode.BlackAutoMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand SwitchToRedAutoModeCommand =>
        CommandMiddlewareHelper.BuildCommand(
            () => this.SwitchInteractMode(EngineInteractiveMode.RedAutoMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand InvokeBestMovementCommand =>
        CommandMiddlewareHelper.BuildCommand(
            new Command(this.InvokeBestMovement, () => !IsEngineAutoMoving),
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand ReverseBoardCommand =>
        CommandMiddlewareHelper.BuildCommand(
            this.ReverseBoard,
            this.CommonCommandMiddleware());

    public ICommand SwitchToAutoModeCommand =>
        CommandMiddlewareHelper.BuildCommand(
            () => this.SwitchInteractMode(EngineInteractiveMode.AutoMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    public ICommand SaveChessManualCommand =>
        CommandMiddlewareHelper.BuildCommand(
            this.SaveChessManual,
            this.CommonCommandMiddleware());

    public ICommand SwitchToSelectedMovementCommand =>
        CommandMiddlewareHelper.BuildCommand(
            new Command<object>(this.SwitchToSelectedMovement),
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    #endregion


    private CommandMiddlewareHelper.CommandMiddleware AutoSaveChessManualMiddleware()
    {
        return delegate(ICommand command)
        {
            return new Command<object>(_ =>
            {
                command.Execute(_);
                this.SaveChessManual();
            });
        };
    }

    private CommandMiddlewareHelper.CommandMiddleware RefreshEngineCommandMiddleware()
    {
        return delegate(ICommand command)
        {
            return new Command<object>(_ =>
            {
                command.Execute(_);
                RefreshUcciEngine();
            });
        };
    }

    private CommandMiddlewareHelper.CommandMiddleware CommonCommandMiddleware(
        [CallerMemberName] string commandName = "")
    {
        return delegate(ICommand command)
        {
            return new Command<object>(_ =>
            {
                this.CursorSelector.Position = null;

                try
                {
                    Logger.LogInformation($"Received command {commandName}");
                    command.Execute(_);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "MilkChess click Event error");
                }

                Logger.LogInformation($"Finished command {commandName}");
            }, command.CanExecute);
        };
    }

    partial void OnEngineChanged(UcciEngineV2 oldValue, UcciEngineV2 newValue)
    {
        OnPropertyChanged(nameof(EngineState));
        OnPropertyChanged(nameof(ConnectOrDisconnectEngineText));
    }

    partial void OnScoreChanged(int oldValue, int newValue)
    {
        OnPropertyChanged(nameof(ScoreColor));
    }

    partial void OnInteractiveModeChanged(EngineInteractiveMode oldValue, EngineInteractiveMode newValue)
    {
        OnPropertyChanged(nameof(SwitchToStopModeColor));
        OnPropertyChanged(nameof(SwitchToAnalyzeModeColor));
        OnPropertyChanged(nameof(SwitchToAutoModeColor));
        OnPropertyChanged(nameof(SwitchToRedAutoModeColor));
        OnPropertyChanged(nameof(SwitchToBlackAutoModeColor));
    }
}