using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Helper;
using MilkChess.Model;
using Color = Microsoft.Maui.Graphics.Color;


namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel
{
    public string ConnectOrDisconnectEngineText => this.InteractiveMode == EngineInteractiveMode.None
        ? UIConstants.ConnectedEngineText
        : UIConstants.DisconnectedEngineText;

    public string SwitchToIdleModeText =>
        this.InteractiveMode is EngineInteractiveMode.IdleMode or EngineInteractiveMode.None
            ? UIConstants.EngineIdleStateText
            : UIConstants.StopEngineText;

    public bool IsChessboardFreeFromAutoMoving => !IsEngineOperatingChessBoard();

    public bool CanInvokeBestMovement => this.InteractiveMode == EngineInteractiveMode.AnalyzeMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveBackwardCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveForwardCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToStartCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToEndCommand))]
    [NotifyCanExecuteChangedFor(nameof(InvokeBestMovementCommand))]
    [NotifyPropertyChangedFor(nameof(SwitchToIdleModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToAnalyzeModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToAutoModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToBlackAutoModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToRedAutoModeColor))]
    [NotifyPropertyChangedFor(nameof(ConnectOrDisconnectEngineText))]
    [NotifyPropertyChangedFor(nameof(SwitchToIdleModeText))]
    private EngineInteractiveMode _interactiveMode = EngineInteractiveMode.None;

    [ObservableProperty] private EngineFeedback _engineFeedback;

    public Color SwitchToIdleModeColor => (this.InteractiveMode == EngineInteractiveMode.IdleMode)
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

    public Rect ExecutionTurnLayoutBound => new Rect(UIConstants.HorizontalOffset / 2.0,
        UIConstants.ChessBoardHeight / 2.0,
        AbsoluteLayout.AutoSize,
        AbsoluteLayout.AutoSize);

    public int ChessBoardHeight => UIConstants.ChessBoardHeight;

    public int ChessBoardWidth => UIConstants.ChessBoardWidth;

    #region command definition

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private async Task OnEditAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.EditAsync,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.AutoSaveChessManualMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private async Task OnConfigureEngineAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.ConfigureEngineAsync,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private void OnReset() =>
        CommandMiddlewareHelper.Execute(
            this.Reset,
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private void OnMoveBackward() =>
        CommandMiddlewareHelper.Execute(
            this.MoveBackward,
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private void OnMoveForward() =>
        CommandMiddlewareHelper.Execute(
            this.MoveForward,
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private void OnMoveToStart() =>
        CommandMiddlewareHelper.Execute(
            this.MoveToStart,
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private void OnMoveToEnd() =>
        CommandMiddlewareHelper.Execute(
            this.MoveToEnd,
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnConnectOrDisconnectEngine() =>
        CommandMiddlewareHelper.Execute(
            this.ConnectOrDisconnectEngine,
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnStopEngine() =>
        CommandMiddlewareHelper.Execute(
            () => this.SwitchInteractMode(EngineInteractiveMode.IdleMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnSwitchToAnalyzeMode() =>
        CommandMiddlewareHelper.Execute(
            () => this.SwitchInteractMode(EngineInteractiveMode.AnalyzeMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnSwitchToBlackAutoMode() =>
        CommandMiddlewareHelper.Execute(
            () => this.SwitchInteractMode(EngineInteractiveMode.BlackAutoMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnSwitchToRedAutoMode() =>
        CommandMiddlewareHelper.Execute(
            () => this.SwitchInteractMode(EngineInteractiveMode.RedAutoMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnSwitchToAutoMode() =>
        CommandMiddlewareHelper.Execute(
            () => this.SwitchInteractMode(EngineInteractiveMode.AutoMode),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand(CanExecute = nameof(CanInvokeBestMovement))]
    private void OnInvokeBestMovement() =>
        CommandMiddlewareHelper.Execute(
            this.InvokeBestMovement,
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnRotateBoard() =>
        CommandMiddlewareHelper.Execute(
            this.RotateBoard,
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnExchangeTurn() =>
        CommandMiddlewareHelper.Execute(
            this.ExchangeTurn,
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnSaveChessManual() =>
        CommandMiddlewareHelper.Execute(
            this.SaveChessManual,
            this.CommonCommandMiddleware());

    [RelayCommand]
    private void OnSwitchToSelectedMovement(object sender) =>
        CommandMiddlewareHelper.BuildCommand(
            new Command<object>(this.SwitchToSelectedMovement),
            this.AutoSaveChessManualMiddleware(),
            this.RefreshEngineCommandMiddleware(),
            this.CommonCommandMiddleware()).Execute(sender);

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

    private CommandMiddlewareHelper.AsyncCommandMiddleware AutoSaveChessManualMiddlewareAsync()
    {
        return next =>
        {
            return async () =>
            {
                this.StopEngine();

                await next();
                SaveChessManual();
            };
        };
    }

    private CommandMiddlewareHelper.CommandMiddleware RefreshEngineCommandMiddleware()
    {
        return delegate(ICommand command)
        {
            return new Command<object>(_ =>
            {
                this.StopEngine();
                command.Execute(_);
                RefreshUciEngine();
            });
        };
    }

    private CommandMiddlewareHelper.AsyncCommandMiddleware RefreshEngineCommandMiddlewareAsync()
    {
        return next =>
        {
            return async () =>
            {
                StopEngine();

                await next();
                RefreshUciEngine();
            };
        };
    }

    private CommandMiddlewareHelper.CommandMiddleware CommonCommandMiddleware(
        [CallerMemberName] string commandName = "")
    {
        return delegate(ICommand command)
        {
            return new Command<object>(sender =>
            {
                this.CursorSelector.Position = null;

                try
                {
                    Logger.LogInformation($"Received command {commandName}");
                    command.Execute(sender);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "MilkChess click Event error");
                }

                Logger.LogInformation($"Finished command {commandName}");
            }, command.CanExecute);
        };
    }

    private CommandMiddlewareHelper.AsyncCommandMiddleware CommonCommandMiddlewareAsync(
        [CallerMemberName] string commandName = "")
    {
        return next =>
        {
            return async () =>
            {
                this.CursorSelector.Position = null;

                try
                {
                    Logger.LogInformation($"Received command {commandName}");
                    await next();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "MilkChess click Event error");
                }

                Logger.LogInformation($"Finished command {commandName}");
            };
        };
    }
}