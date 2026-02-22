using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Model;
using MilkChess.Ucci;

namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel
{
    [ObservableProperty] private UcciEngineV2 engine;

    private CancellationTokenSource EngineCancellationTokenSource;

    private Task EngineExecutionTask;

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
        Engine = ServiceProvider.GetService<UcciEngineV2>();

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

        RefreshUcciEngine();
    }

    private void RefreshUcciEngine()
    {
        if (this.Engine == null) return;

        Engine.Stop();

        // Don't clear EngineSuggestedMovements ff simply stopping the engine
        if (InteractiveMode != EngineInteractiveMode.StopMode)
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
                this.Score = feedback.Score;

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

    private void DisconnectEngine()
    {
        Engine.Stop();
        Engine.Quit();

        Engine = null;

        MovementIndicator.Reset();
        EngineSuggestedMovements.Clear();
    }

    public enum EngineInteractiveMode
    {
        StopMode,
        AnalyzeMode,
        AutoMode,
        RedAutoMode,
        BlackAutoMode,
    }
    //
    //
    // private void HandleInfoFeedback_dep(InfoFeedback feedback)
    // {
    //     Logger.LogDebug("[HandleInfoFeedback] {}", feedback);
    //
    //     int score = 0, depth = -1;
    //
    //     if (feedback.Info.ContainsKey("score") &&
    //         int.TryParse(feedback.Info["score"], out score))
    //     {
    //         if (HistoryManager.ExecutionTurn == Model.Color.BLACK)
    //         {
    //             score = -score;
    //         }
    //
    //         this.DispatchAction(() => { this.Score = score; });
    //     }
    //
    //     if (feedback.Info.ContainsKey("depth"))
    //     {
    //         int.TryParse(feedback.Info["depth"], out depth);
    //     }
    //
    //     if (feedback.Pv?.Count > 0)
    //     {
    //         StringBuilder predict = new StringBuilder();
    //
    //         ChessModel chessModel = new ChessModel(this);
    //
    //         for (int i = 0; i < feedback.Pv.Count; i++)
    //         {
    //             if (!MovementExtension.ParseMovementFromUnifyString(feedback.Pv[i], out Movement movement))
    //             {
    //                 Logger.LogError("Error parsing movement {}", feedback.Pv[i]);
    //                 break;
    //             }
    //
    //             chessModel.PopulateMovement(movement);
    //
    //             if (i == 0 && MovementIndicator.Movement?.ToUnifyString() != movement.ToUnifyString()) // best movement
    //             {
    //                 this.DispatchAction(() => MovementIndicator.SetMovement(movement));
    //             }
    //
    //             predict.Append(" ").Append(movement.ChineseViewableString);
    //             chessModel.ApplyMovement(movement);
    //         }
    //
    //         this.DispatchAction(() => EngineSuggestedMovements.Insert(0, new PredictMovementTrace()
    //         {
    //             Score = score,
    //             Depth = depth,
    //             Movements = predict.ToString(),
    //         }));
    //
    //         Logger.LogInformation("Engine suggest: {} {}", score, predict);
    //     }
    //
    //     Logger.LogInformation("Finished HandleInfoFeedback");
    // }
    //
    // public void NotifyRefreshEngineFeedback()
    // {
    //     if (this.InteractiveMode != EngineInteractiveMode.StopMode && this.EngineExecutionTask == null)
    //     {
    //         uint requestId = Interlocked.Increment(ref LastEngineRequestId);
    //         Logger.LogInformation("Start NotifyRefreshEngineFeedback {}", requestId);
    //
    //         EngineSuggestedMovements.Clear();
    //         MovementIndicator.ClearMovement();
    //
    //         GoCancellationTokenSource = new CancellationTokenSource();
    //         EngineExecutionTask = Task.Factory.StartNew(() =>
    //             this.BlockingNotifyRefreshEngineFeedback(requestId, GoCancellationTokenSource.Token));
    //
    //         Logger.LogTrace("End NotifyRefreshEngineFeedback {}", requestId);
    //     }
    // }
    //
    // private void BlockingNotifyRefreshEngineFeedback(uint requestId, CancellationToken cancellationToken)
    // {
    //     Logger.LogTrace("Start BlockingNotifyRefreshEngineFeedback {}", requestId);
    //     bool recursiveQueryEngine;
    //     int queryCount = 0;
    //
    //     do
    //     {
    //         recursiveQueryEngine = false;
    //         queryCount++;
    //
    //         int depth;
    //         bool immediateInvokeBestMove;
    //
    //         if (InteractiveMode == EngineInteractiveMode.AnalyzeMode
    //             || (InteractiveMode == EngineInteractiveMode.BlackAutoMode &&
    //                 HistoryManager.ExecutionTurn == Model.Color.RED)
    //             || (InteractiveMode == EngineInteractiveMode.RedAutoMode &&
    //                 HistoryManager.ExecutionTurn == Model.Color.BLACK))
    //         {
    //             depth = -1;
    //             immediateInvokeBestMove = false;
    //         }
    //         else
    //         {
    //             depth = this.Config.Engine.GoDepth;
    //             immediateInvokeBestMove = true;
    //             IsEngineAutoMoving = true;
    //         }
    //
    //         Engine?.Position(this, cancellationToken);
    //         var bestMove = Engine?.Go(new GoOption()
    //             {
    //                 Depth = depth,
    //             },
    //             cancellationToken,
    //             delegate(object sender, InfoFeedback feedback)
    //             {
    //                 // there may be multiple thread running in the same time
    //                 // only update UI for the last engine request
    //                 if (requestId == LastEngineRequestId && !cancellationToken.IsCancellationRequested)
    //                 {
    //                     HandleInfoFeedback(feedback);
    //                 }
    //             });
    //
    //         if (bestMove != null && !cancellationToken.IsCancellationRequested && requestId == LastEngineRequestId)
    //         {
    //             if (MovementExtension.ParseMovementFromUnifyString(bestMove.BestMovement, out Movement bestMovement))
    //             {
    //                 Logger.LogInformation("{} Received Best move {}", requestId, bestMove.BestMovement);
    //
    //                 this.PopulateMovementWithContext(bestMovement);
    //
    //                 if (immediateInvokeBestMove)
    //                 {
    //                     int count = queryCount;
    //                     CountdownEvent dispatchCountDownEvent = new CountdownEvent(1);
    //
    //                     this.DispatchAction(() =>
    //                     {
    //                         this.ApplyMovement(bestMovement);
    //                         HistoryManager.InsertMovement(bestMovement);
    //                         this.CursorSelector.Position = null;
    //                         Logger.LogDebug("{requestId}.{queryCount} Executed movement {}, turn {}", requestId, count,
    //                             bestMovement,
    //                             HistoryManager.ExecutionTurn);
    //
    //                         EngineSuggestedMovements.Clear();
    //                         MovementIndicator.ClearMovement();
    //
    //                         dispatchCountDownEvent.Signal();
    //                     });
    //
    //                     // wait for the dispatch action to finished
    //                     try
    //                     {
    //                         dispatchCountDownEvent.Wait(cancellationToken);
    //                     }
    //                     catch (OperationCanceledException)
    //                     {
    //                     }
    //
    //                     this.SaveChessManual();
    //
    //                     // indicates to query engine again
    //                     recursiveQueryEngine = true;
    //                 }
    //                 else
    //                 {
    //                     this.DispatchAction(() => { MovementIndicator.SetMovement(bestMovement); });
    //                 }
    //             }
    //             else
    //             {
    //                 Logger.LogError($"Unable to parse best movement {bestMove.BestMovement}");
    //             }
    //         }
    //     } while (recursiveQueryEngine && !cancellationToken.IsCancellationRequested);
    //
    //     IsEngineAutoMoving = false;
    //
    //     Logger.LogTrace("End BlockingNotifyRefreshEngineFeedback {}", requestId);
    // }
}