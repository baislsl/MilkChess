using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Model;
using MilkChess.Protocol;

namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InvokeBestMovementCommand))]
    [NotifyPropertyChangedFor(nameof(ConnectOrDisconnectEngineText))]
    private UciEngine _engine;

    private void DispatchAction(Action action, bool wait = false)
    {
        if (Application.Current == null || Application.Current.MainPage == null)
        {
            Logger.LogError("Null application context");
            return;
        }

        CountdownEvent dispatchCountDownEvent = null;
        var dispatchAction = action;

        if (wait)
        {
            dispatchCountDownEvent = new CountdownEvent(1);
            dispatchAction = () =>
            {
                action();
                dispatchCountDownEvent.Signal();
            };
        }

        Application.Current.MainPage.Dispatcher.Dispatch(dispatchAction);
        dispatchCountDownEvent?.Wait();
    }

    public async Task ConnectOrDisconnectEngineAsync()
    {
        if (this.Engine == null)
        {
            try
            {
                await this.InitializeUciEngineAsync();
            }
            catch (Exception ex)
            {
                // Setting engine back to null
                this.Engine = null;

                await AlertService.PopAlertAsync($"Error initializing Uci engine, {ex}");
                throw;
            }
        }
        else
        {
            await this.DisconnectEngineAsync();
        }
    }

    public async Task InitializeUciEngineAsync()
    {
        Logger.LogInformation("Initailzing engine...");
        if (Config.DefaultEngineConfig == null)
        {
            await AlertService.PopAlertAsync("No engine config selected");
            return;
        }

        Engine = ServiceProvider.GetService<UciEngine>();

        CancellationTokenSource cancellationTokenSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(this.Config.DefaultEngineConfig.ConnectionTimeoutSeconds));

        var startUciFeedback = await Engine.StartUciAsync(cancellationTokenSource.Token); // EngineState = Connected

        if (!startUciFeedback.ReceivedUciOk)
        {
            throw new Exception("Uci start failed");
        }
        else
        {
            Logger.LogInformation($"init engine success");
        }

        if (this.Config.DefaultEngineConfig?.Threads > 0)
        {
            await Engine.SetOptionAsync("Threads", this.Config.DefaultEngineConfig.Threads.ToString(),
                cancellationTokenSource.Token);
        }

        if (this.Config.EngineOption?.Count > 0)
        {
            foreach (var option in this.Config.EngineOption)
            {
                await Engine.SetOptionAsync(option.Key, option.Value, cancellationTokenSource.Token);
            }
        }

        await Engine.WaitToReadyAsync(cancellationTokenSource.Token); // EngineState = Ready

        if (this.InteractiveMode == EngineInteractiveMode.None)
        {
            this.InteractiveMode = EngineInteractiveMode.IdleMode;
        }

        await RefreshUciEngineAsync();
    }

    private async Task RefreshUciEngineAsync()
    {
        if (this.Engine == null) return;

        await this.StopEngineAsync();

        // Don't clear EngineFeedback ff simply stopping the engine
        if (InteractiveMode != EngineInteractiveMode.IdleMode)
        {
            EngineFeedback.Clear();
        }

        var option = new GoOption();
        if (InteractiveMode == EngineInteractiveMode.AnalyzeMode)
        {
            option.Depth = -1;
            await Engine.GoInfiniteAsync(option, this, this.OnFeedbackReceived);
        }
        else if (InteractiveMode == EngineInteractiveMode.AutoMode)
        {
            option.Depth = this.Config.DefaultEngineConfig.GoDepth;
            await Engine.ExecuteForeverAsync(option, this, this.OnFeedbackReceived, this.OnBestMovementReceived);
        }
        else if (InteractiveMode == EngineInteractiveMode.BlackAutoMode ||
                 InteractiveMode == EngineInteractiveMode.RedAutoMode)
        {
            var engineOperateColor = InteractiveMode == EngineInteractiveMode.RedAutoMode
                ? MilkChess.Model.Color.RED
                : MilkChess.Model.Color.BLACK;

            if (engineOperateColor == HistoryManager.ExecutionTurn)
            {
                option.Depth = this.Config.DefaultEngineConfig.GoDepth;
                await Engine.ExecuteOnceAsync(option, this, this.OnFeedbackReceived, this.OnBestMovementReceived);
            }
        }
    }

    private void OnFeedbackReceived(InfoFeedback feedback)
    {
        Logger.LogDebug("[HandleInfoFeedback] {}", feedback);
        this.DispatchAction(() =>
        {
            if (feedback.Movements?.Count > 0)
            {
                if (MovementIndicator.Movement?.ToUnifyString() != feedback.Movements[0].ToUnifyString())
                {
                    // Set best movement
                    MovementIndicator.SetMovement(feedback.Movements[0]);
                }

                EngineFeedback.Insert(new PredictMovementTrace()
                {
                    Score = feedback.Score,
                    Depth = feedback.Depth,
                    Movements = feedback.ViewableMovementList,
                });
            }
        }, false);
    }

    private void OnBestMovementReceived(BestMove bestMove)
    {
        if (this.ParseUnifyStringMovement(bestMove.BestMovement, out Movement bestMovement))
        {
            Logger.LogInformation("Getting Best move {}", bestMove.BestMovement);


            this.DispatchAction(() =>
            {
                Logger.LogInformation("Invoke Best move {}", bestMove.BestMovement);

                this.ApplyMovement(bestMovement);
                HistoryManager.InsertMovement(bestMovement);
                this.CursorSelector.Position = null;

                EngineFeedback.Clear();
            }, true);
        }
        else
        {
            Logger.LogError($"Unable to parse best movement {bestMove.BestMovement}");

            throw new Exception($"Unable to parse best movement {bestMove.BestMovement}");
        }
    }

    private async Task StopEngineAsync()
    {
        if (Engine != null)
        {
            await Engine.StopAsync();
        }
    }

    private async Task DisconnectEngineAsync()
    {
        await this.StopEngineAsync();

        await Engine.QuitAsync();

        Engine = null;
        this.InteractiveMode = EngineInteractiveMode.None;

        MovementIndicator.Reset();
        EngineFeedback.Clear();
    }

    public enum EngineInteractiveMode
    {
        None,
        IdleMode,
        AnalyzeMode,
        AutoMode,
        RedAutoMode,
        BlackAutoMode,
    }
}