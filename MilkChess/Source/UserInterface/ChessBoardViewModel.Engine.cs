using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Model;
using MilkChess.Protocol;

namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel
{
    [ObservableProperty] private UciEngine _engine;

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

        bool result = Application.Current.MainPage.Dispatcher.Dispatch(dispatchAction);

        if (!result)
        {
            Logger.LogError("Failed to dispatch action");
        }
        else
        {
            dispatchCountDownEvent?.Wait();
        }
    }

    public void ConnectOrDisconnectEngine()
    {
        if (this.Engine == null)
        {
            this.InitializeUciEngine();
        }
        else
        {
            this.DisconnectEngine();
        }
    }

    public void InitializeUciEngine()
    {
        Logger.LogInformation("Initailzing engine...");
        Engine = ServiceProvider.GetService<UciEngine>();

        CancellationTokenSource cancellationTokenSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(this.Config.Engine.ConnectionTimeoutSeconds));

        var startUciFeedback = Engine.StartUci(cancellationTokenSource.Token); // EngineState = Connected

        if (!startUciFeedback.ReceivedUciOk)
        {
            throw new Exception("Uci start failed");
        }
        else
        {
            Logger.LogInformation($"init engine success");
        }

        if (this.Config.EngineOption?.Count > 0)
        {
            foreach (var option in this.Config.EngineOption)
            {
                Engine.SetOption(option.Key, option.Value, cancellationTokenSource.Token);
            }
        }

        Engine.WaitToReady(cancellationTokenSource.Token); // EngineState = Ready

        this.InteractiveMode = EngineInteractiveMode.IdleMode;

        RefreshUciEngine();
    }

    private void RefreshUciEngine()
    {
        if (this.Engine == null) return;

        this.StopEngine();

        // Don't clear EngineFeedback ff simply stopping the engine
        if (InteractiveMode != EngineInteractiveMode.IdleMode)
        {
            EngineFeedback.Clear();
        }

        var option = new GoOption();
        if (InteractiveMode == EngineInteractiveMode.AnalyzeMode)
        {
            option.Depth = -1;
            Engine.GoInfinite(option, this, this.onFeedbackReceived);
        }
        else if (InteractiveMode == EngineInteractiveMode.AutoMode)
        {
            option.Depth = this.Config.Engine.GoDepth;
            Engine.ExecuteForever(option, this, this.onFeedbackReceived, this.onBestMovementReceived);
        }
        else if (InteractiveMode == EngineInteractiveMode.BlackAutoMode ||
                 InteractiveMode == EngineInteractiveMode.RedAutoMode)
        {
            var engineOperateColor = InteractiveMode == EngineInteractiveMode.RedAutoMode
                ? MilkChess.Model.Color.RED
                : MilkChess.Model.Color.BLACK;

            if (engineOperateColor == HistoryManager.ExecutionTurn)
            {
                option.Depth = this.Config.Engine.GoDepth;
                Engine.ExecuteOnce(option, this, this.onFeedbackReceived, this.onBestMovementReceived);
            }
        }
    }

    private void onFeedbackReceived(InfoFeedback feedback)
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

    private void onBestMovementReceived(BestMove bestMove)
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

            this.SaveChessManual();
        }
        else
        {
            Logger.LogError($"Unable to parse best movement {bestMove.BestMovement}");

            throw new Exception($"Unable to parse best movement {bestMove.BestMovement}");
        }
    }

    private void StopEngine()
    {
        Engine?.Stop();
    }

    private void DisconnectEngine()
    {
        this.StopEngine();

        Engine.Quit();

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