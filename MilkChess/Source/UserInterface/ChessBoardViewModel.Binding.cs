using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Model;
using Color = Microsoft.Maui.Graphics.Color;

namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel
{
    #region view model definition

    public Color ScoreColor => this.Score > 0 ? Colors.Red : Colors.Black;

    private int _score;

    public int Score
    {
        get => _score;
        set
        {
            this._score = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Score)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScoreColor)));
        }
    }

    private bool _IsEngineAutoMoving;

    private bool IsEngineAutoMoving
    {
        get => _IsEngineAutoMoving;
        set
        {
            if (value != _IsEngineAutoMoving)
            {
                _IsEngineAutoMoving = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResetCommand)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveBackwardCommand)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveForwardCommand)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveToStartCommand)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveToEndCommand)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResetCommand)));
            }
        }
    }

    public ObservableCollection<PredictMovementTrace> EngineSuggestedMovements { get; set; }

    public int ChessBoardHeight => UIConstants.ChessBoardHeight;

    public int ChessBoardWidth => UIConstants.ChessBoardWidth;

    public ObservableCollection<Movement> HistoryMovements => (ObservableCollection<Movement>) HistoryManager.Movements;

    public Color SwitchToAnalyzeModeColor => this.InteractiveMode == EngineInteractiveMode.AnalyzeMode
                    ? ActiveInteractiveModeColor
                    : InactiveInteractiveModeColor;

    public Color SwitchToBlackAutoModeColor => this.InteractiveMode == EngineInteractiveMode.BlackAutoMode
                    ? ActiveInteractiveModeColor
                    : InactiveInteractiveModeColor;

    public Color SwitchToRedAutoModeColor => this.InteractiveMode == EngineInteractiveMode.RedAutoMode
                    ? ActiveInteractiveModeColor
                    : InactiveInteractiveModeColor;

    public Color SwitchToAutoModeColor => this.InteractiveMode == EngineInteractiveMode.AutoMode
                    ? ActiveInteractiveModeColor
                    : InactiveInteractiveModeColor;

    // Compiler will throw error result if I remove this unused fields.
    // This seems to be the compiler bug.
    public string ChineseViewableString => default;
    public string ActionCount => default;
    public string Movements => default;
    public string Depth => default;

    #endregion

    #region command definition

    public ICommand ResetCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    new Command(this.Reset, () => !IsEngineAutoMoving),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand MoveBackwardCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    new Command(this.MoveBackward, () => !IsEngineAutoMoving),
                                    this.AutoSaveChessManualMiddleware(),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand MoveForwardCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    new Command(this.MoveForward, () => !IsEngineAutoMoving),
                                    this.AutoSaveChessManualMiddleware(),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand MoveToStartCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    new Command(this.MoveToStart, () => !IsEngineAutoMoving),
                                    this.AutoSaveChessManualMiddleware(),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand MoveToEndCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    new Command(this.MoveToEnd, () => !IsEngineAutoMoving),
                                    this.AutoSaveChessManualMiddleware(),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand ConnectToRemoteEngineCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    this.InitializeUcciEngine,
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand DisconnectEngineCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    this.DisconnectEngine,
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand InvokeBestMovementCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    new Command(this.InvokeBestMovement, () => !IsEngineAutoMoving),
                                    this.AutoSaveChessManualMiddleware(),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand ReverseBoardCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    this.ReverseBoard,
                                    this.LogAndCatchCommandMiddleware());


    public ICommand SwitchToAnalyzeModeCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    () => this.SwitchInteractMode(EngineInteractiveMode.AnalyzeMode),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand SwitchToBlackAutoModeCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    () => this.SwitchInteractMode(EngineInteractiveMode.BlackAutoMode),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand SwitchToRedAutoModeCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    () => this.SwitchInteractMode(EngineInteractiveMode.RedAutoMode),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand SwitchToAutoModeCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    () => this.SwitchInteractMode(EngineInteractiveMode.AutoMode),
                                    this.RefreshEngineCommandMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

    public ICommand SaveChessManualCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    this.SaveChessManual,
                                    this.LogAndCatchCommandMiddleware());

    public ICommand SwitchToSelectedMovementCommand =>
                    CommandMiddlewareHelper.BuildCommand(
                                    new Command<object>(this.SwitchToSelectedMovement),
                                    this.AutoSaveChessManualMiddleware(),
                                    this.LogAndCatchCommandMiddleware());

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
                StopEngineRunner();
                command.Execute(_);
                NotifyRefreshEngineFeedback();
            });
        };
    }

    private CommandMiddlewareHelper.CommandMiddleware LogAndCatchCommandMiddleware(
                    [CallerMemberName] string commandName = "")
    {
        return delegate(ICommand command)
        {
            return new Command<object>(_ =>
            {
                try
                {
                    Logger.LogInformation($"Received command {commandName}");
                    command.Execute(_);
                    Logger.LogInformation($"Received command {commandName}");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "MilkChess click Event error");
                }

                Logger.LogInformation($"Finished command {commandName}");
            }, command.CanExecute);
        };
    }
}