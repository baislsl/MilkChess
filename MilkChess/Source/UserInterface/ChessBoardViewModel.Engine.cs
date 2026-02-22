using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Model;
using MilkChess.Ucci;

namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel
{
    [ObservableProperty]
    private UcciEngine _engine;

    private void DispatchAction(Action action)
    {
        if (Application.Current == null || Application.Current.MainPage == null)
        {
            Logger.LogError("Null application context");
            return;
        }

        Application.Current.MainPage.Dispatcher.Dispatch(action);
    }

    public void ConnectOrDisconnectEngine()
    {
        if (this.Engine == null)
        {
            this.InitializeUcciEngine();
        }
        else
        {
            this.DisconnectEngine();
        }
    }

    public void InitializeUcciEngine()
    {
        Logger.LogInformation("Initailzing engine...");
        Engine = ServiceProvider.GetService<UcciEngine>();

        CancellationTokenSource cancellationTokenSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(this.Config.Engine.ConnectionTimeoutSeconds));

        var startUciFeedback = Engine.StartUci(cancellationTokenSource.Token); // EngineState = Connected

        if (!startUciFeedback.ReceivedUcciOk)
        {
            throw new Exception("Ucci start failed");
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

        RefreshUcciEngine();
    }

    private void RefreshUcciEngine()
    {
        if (this.Engine == null) return;

        this.StopEngine();

        // Don't clear EngineSuggestedMovements ff simply stopping the engine
        if (InteractiveMode != EngineInteractiveMode.IdleMode)
        {
            EngineSuggestedMovements.Clear();
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

                EngineSuggestedMovements.Insert(0, new PredictMovementTrace()
                {
                    Score = feedback.Score,
                    Depth = feedback.Depth,
                    Movements = feedback.ViewableMovementList,
                });
            }
        });
    }

    private void onBestMovementReceived(BestMove bestMove)
    {
        if (this.ParseUnifyStringMovement(bestMove.BestMovement, out Movement bestMovement))
        {
            Logger.LogInformation("Getting Best move {}", bestMove.BestMovement);

            var dispatchCountDownEvent = new CountdownEvent(1);

            this.DispatchAction(() =>
            {
                Logger.LogInformation("Invoke Best move {}", bestMove.BestMovement);

                this.ApplyMovement(bestMovement);
                HistoryManager.InsertMovement(bestMovement);
                this.CursorSelector.Position = null;

                EngineSuggestedMovements.Clear();

                dispatchCountDownEvent.Signal();
            });

            dispatchCountDownEvent.Wait();
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
        EngineSuggestedMovements.Clear();
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