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
    public bool IsChessboardFreeFromAutoMoving => !IsEngineOperatingChessBoard();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveBackwardCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveForwardCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToStartCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToEndCommand))]
    [NotifyPropertyChangedFor(nameof(SwitchToIdleModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToAnalyzeModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToAutoModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToBlackAutoModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToRedAutoModeColor))]
    [NotifyPropertyChangedFor(nameof(SwitchToIdleModeText))]
    private EngineInteractiveMode _interactiveMode = EngineInteractiveMode.None;

    public string ConnectOrDisconnectEngineText => this.Engine == null
        ? UIConstants.ConnectedEngineText
        : UIConstants.DisconnectedEngineText;

    public string SwitchToIdleModeText =>
        this.InteractiveMode is EngineInteractiveMode.IdleMode or EngineInteractiveMode.None || this.Engine == null
            ? UIConstants.EngineIdleStateText
            : UIConstants.StopEngineText;

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

    [RelayCommand]
    private async Task OnConfigureEngineAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.ConfigureEngineAsync,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private async Task OnResetAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.Reset,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private async Task OnMoveBackwardAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.MoveBackward,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private async Task OnMoveForward() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.MoveForward,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private async Task OnMoveToStartAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.MoveToStart,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand(CanExecute = nameof(IsChessboardFreeFromAutoMoving))]
    private async Task OnMoveToEndAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.MoveToEnd,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnConnectOrDisconnectEngineAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.ConnectOrDisconnectEngineAsync,
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnStopEngineAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            () => this.SwitchInteractMode(EngineInteractiveMode.IdleMode),
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnSwitchToAnalyzeModeAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            () => this.SwitchInteractMode(EngineInteractiveMode.AnalyzeMode),
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnSwitchToBlackAutoMode() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            () => this.SwitchInteractMode(EngineInteractiveMode.BlackAutoMode),
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnSwitchToRedAutoMode() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            () => this.SwitchInteractMode(EngineInteractiveMode.RedAutoMode),
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnSwitchToAutoMode() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            () => this.SwitchInteractMode(EngineInteractiveMode.AutoMode),
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnInvokeBestMovementAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.InvokeBestMovement,
            this.AutoSaveChessManualMiddlewareAsync(),
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private void OnRotateBoard() =>
        CommandMiddlewareHelper.Execute(
            this.RotateBoard,
            this.CommonCommandMiddleware());

    [RelayCommand]
    private async Task OnExchangeTurnAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.ExchangeTurn,
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private void OnSaveChessManual() =>
        CommandMiddlewareHelper.Execute(
            this.SaveChessManual,
            this.CommonCommandMiddleware());

    [RelayCommand]
    private async Task OnCopyFenAsync() =>
        await CommandMiddlewareHelper.ExecuteAsync(
            this.CopyFenAsync,
            this.CommonCommandMiddlewareAsync());

    [RelayCommand]
    private async Task OnSwitchToSelectedMovement(object sender) =>
        await CommandMiddlewareHelper.ExecuteAsync(
            () => this.SwitchToSelectedMovement(sender),
            this.RefreshEngineCommandMiddlewareAsync(),
            this.CommonCommandMiddlewareAsync());

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
                await this.StopEngineAsync();

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
                StopEngineAsync().GetAwaiter().GetResult();
                command.Execute(_);
                RefreshUciEngineAsync().GetAwaiter().GetResult();
            });
        };
    }

    private CommandMiddlewareHelper.AsyncCommandMiddleware RefreshEngineCommandMiddlewareAsync()
    {
        return next =>
        {
            return async () =>
            {
                await StopEngineAsync();
                await next();
                await RefreshUciEngineAsync();
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