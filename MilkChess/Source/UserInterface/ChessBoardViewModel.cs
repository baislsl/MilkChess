using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Ucci;
using Color = Microsoft.Maui.Graphics.Color;

namespace MilkChess.UserInterface;

public class ChessBoardViewModel : ChessModel, IChessBoardContext, INotifyPropertyChanged
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

    public ICommand ResetCommand => this.WrapEngineInteractCommand(this.Reset, _ => !IsEngineAutoMoving);

    public ICommand MoveBackwardCommand => this.WrapEngineInteractCommand(this.MoveBackward, _ => !IsEngineAutoMoving);

    public ICommand MoveForwardCommand => this.WrapEngineInteractCommand(this.MoveForward, _ => !IsEngineAutoMoving);

    public ICommand MoveToStartCommand => this.WrapEngineInteractCommand(this.MoveToStart, _ => !IsEngineAutoMoving);

    public ICommand MoveToEndCommand => this.WrapEngineInteractCommand(this.MoveToEnd, _ => !IsEngineAutoMoving);

    public ICommand ConnectToRemoteEngineCommand => this.WrapEngineInteractCommand(this.InitializeUcciEngine);

    public ICommand DisconnectEngineCommand => this.WrapEngineInteractCommand(this.DisconnectEngine);

    public ICommand InvokeBestMovementCommand =>
                    this.WrapEngineInteractCommand(this.InvokeBestMovement, _ => !IsEngineAutoMoving);

    public ICommand ReverseBoardCommand => this.WrapClickCommand(this.ReverseBoard);

    public ICommand SwitchToAnalyzeMode =>
                    this.WrapEngineInteractCommand(() => this.SwitchInteractMode(EngineInteractiveMode.AnalyzeMode));

    public ICommand SwitchToBlackAutoMode =>
                    this.WrapEngineInteractCommand(() => this.SwitchInteractMode(EngineInteractiveMode.BlackAutoMode));
    
    public ICommand SwitchToRedAutoMode =>
                    this.WrapEngineInteractCommand(() => this.SwitchInteractMode(EngineInteractiveMode.RedAutoMode));

    public ICommand SwitchToAutoMode =>
                    this.WrapEngineInteractCommand(() => this.SwitchInteractMode(EngineInteractiveMode.AutoMode));

    #endregion

    #region binding component

    public event PropertyChangedEventHandler PropertyChanged;

    public readonly CursorSelector CursorSelector;

    public ChessButton SelectedChess;

    #endregion

    public readonly List<List<ChessButton>> Matrix;

    public new readonly ILogger Logger;

    private readonly IServiceProvider ServiceProvider;

    public readonly MovementIndicator MovementIndicator;

    private UcciEngine Engine;

    private CancellationTokenSource GoCancellationTokenSource;

    private uint LastEngineRequestId = 0;

    private Task EngineExecutionTask;

    private EngineInteractiveMode InteractiveMode = EngineInteractiveMode.AnalyzeMode;

    private static readonly Color ActiveInteractiveModeColor = Colors.LightBlue;

    private static readonly Color InactiveInteractiveModeColor = Colors.White;

    public HistoryManager HistoryManager { get; }

    public ChessBoardViewModel(
                    IServiceProvider serviceProvider,
                    ILogger<ChessBoardViewModel> logger,
                    HistoryManager historyManager)
                    : base(serviceProvider.GetService<ILogger<ChessModel>>())
    {
        Logger = logger;
        HistoryManager = historyManager;
        ServiceProvider = serviceProvider;

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        foreach (var kv in config.AsEnumerable())
        {
            Logger.LogInformation("Config  {} -> {}", kv.Key, kv.Value);
        }

        Matrix = new List<List<ChessButton>>(ChessBoardHeight);
        for (int i = 0; i < ChessBoardHeight; i++)
        {
            Matrix.Add(new List<ChessButton>(new ChessButton[ChessBoardWidth]));
        }

        Iterator.IterateOverChessBoard(position =>
        {
            Chess chess = this.GetChessAtPosition(position);
            ChessButton chessButton = new ChessButton(chess, position);

            // chessButton.Released += this.WrapEventHandler(this.OnChessSelected);
            chessButton.Command = this.WrapClickCommand(() => { this.OnChessSelected(chessButton, null); });
            Matrix[position.Y][position.X] = chessButton;
        });

        CursorSelector = new CursorSelector(new Position(0, 0), UIConstants.CursorBackgroundResource);
        MovementIndicator = new MovementIndicator();
        EngineSuggestedMovements = new ObservableCollection<PredictMovementTrace>();
    }

    public void AddToAbsoluteLayout(AbsoluteLayout layout)
    {
        MovementIndicator.AddToAbsoluteLayout(layout);

        Iterator.IterateOverChessBoard(position => { layout.Add(Matrix[position.Y][position.X]); });

        layout.Add(CursorSelector);
    }

    public void OnChessSelected(object sender, EventArgs args)
    {
        ChessButton clickedButton = (ChessButton) sender;
        Logger.LogInformation(
                        $"Clicked {clickedButton.Position} {clickedButton.Chess}; Selected {SelectedChess?.Position}, {SelectedChess?.Chess}");

        if (SelectedChess?.Chess == null
            || SelectedChess.Chess.Color != HistoryManager.ExecutionTurn
            || !this.ValidateMovement(SelectedChess.Position, clickedButton.Position))
        {
            SelectedChess = clickedButton;
            CursorSelector.Position = clickedButton.Position;

            Logger.LogInformation($"MoveSelector {SelectedChess.Chess} {CursorSelector.Position}");
        }
        else
        {
            Movement movement = new Movement
            {
                            MovedChess = SelectedChess.Chess,
                            RemovedChess = clickedButton.Chess,
                            From = SelectedChess.Position,
                            To = clickedButton.Position,
                            ActionCount = HistoryManager.Cursor / 2 + 1,
            };

            // 存下招法中文表示
            movement.ChineseViewableString =
                            movement.ToChineseViewableString(this);

            Logger.LogInformation("Execute movement {}", movement);

            StopEngineRunner();

            this.ApplyMovement(movement);
            HistoryManager.InsertMovement(movement);

            SelectedChess = null;
            CursorSelector.Position = clickedButton.Position;

            NotifyRefreshEngineFeedback();
        }
    }

    private void HandleInfoFeedback(InfoFeedback feedback)
    {
        Logger.LogDebug("[HandleInfoFeedback] {}", feedback);

        int score = 0, depth = -1;

        if (feedback.Info.ContainsKey("score") &&
            int.TryParse(feedback.Info["score"], out score))
        {
            if (HistoryManager.ExecutionTurn == Model.Color.BLACK)
            {
                score = -score;
            }

            this.DispatchAction(() => { this.Score = score; });
        }

        if (feedback.Info.ContainsKey("depth"))
        {
            int.TryParse(feedback.Info["depth"], out depth);
        }

        if (feedback.Pv?.Count > 0)
        {
            StringBuilder predict = new StringBuilder();

            ChessModel chessModel = new ChessModel(ServiceProvider.GetRequiredService<ILogger<ChessModel>>(), this);

            for (int i = 0; i < feedback.Pv.Count; i++)
            {
                if (!MovementExtension.ParseMovementFromUnifyString(feedback.Pv[i], out Movement movement))
                {
                    Logger.LogError("Error parsing movement {}", feedback.Pv[i]);
                    break;
                }

                chessModel.PopulateMovement(movement);

                if (i == 0 && MovementIndicator.Movement?.ToUnifyString() != movement.ToUnifyString()) // best movement
                {
                    this.DispatchAction(() => MovementIndicator.SetMovement(movement));
                }

                predict.Append(" ").Append(movement.ChineseViewableString);
                chessModel.ApplyMovement(movement);
            }

            this.DispatchAction(() => EngineSuggestedMovements.Insert(0, new PredictMovementTrace()
            {
                            Score = score,
                            Depth = depth,
                            Movements = predict.ToString(),
            }));

            Logger.LogInformation("Engine suggest: {} {}", score, predict);
        }

        Logger.LogInformation("Finished HandleInfoFeedback");
    }

    public void StopEngineRunner()
    {
        GoCancellationTokenSource?.Cancel();
        EngineExecutionTask?.Wait();
    }

    public void NotifyRefreshEngineFeedback()
    {
        uint requestId = Interlocked.Increment(ref LastEngineRequestId);
        Logger.LogDebug("Start NotifyRefreshEngineFeedback {}", requestId);

        EngineSuggestedMovements.Clear();
        MovementIndicator.ClearMovement();

        GoCancellationTokenSource = new CancellationTokenSource();
        EngineExecutionTask = Task.Factory.StartNew(
                        () => this.BlockingNotifyRefreshEngineFeedback(requestId, GoCancellationTokenSource.Token));

        Logger.LogDebug("End NotifyRefreshEngineFeedback {}", requestId);
    }

    private void BlockingNotifyRefreshEngineFeedback(uint requestId, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Start BlockingNotifyRefreshEngineFeedback {}", requestId);
        bool recursiveQueryEngine;
        int queryCount = 0;

        do
        {
            recursiveQueryEngine = false;
            queryCount++;

            int depth;
            bool immediateInvokeBestMove;

            if (InteractiveMode == EngineInteractiveMode.AnalyzeMode
                || (InteractiveMode == EngineInteractiveMode.BlackAutoMode &&
                    HistoryManager.ExecutionTurn == Model.Color.RED)
                || (InteractiveMode == EngineInteractiveMode.RedAutoMode &&
                    HistoryManager.ExecutionTurn == Model.Color.BLACK))
            {
                depth = -1;
                immediateInvokeBestMove = false;
            }
            else
            {
                depth = Globals.Config.GetValue<int>(Keys.EngineGoDepth);
                immediateInvokeBestMove = true;
                IsEngineAutoMoving = true;
            }


            Engine?.Position(this, cancellationToken);
            var bestMove = Engine?.Go(new GoOption()
                            {
                                            Depth = depth,
                            },
                            cancellationToken,
                            delegate(object sender, InfoFeedback feedback)
                            {
                                // there may be multiple thread running in the same time
                                // only update UI for the last engine request
                                if (requestId == LastEngineRequestId)
                                {
                                    HandleInfoFeedback(feedback);
                                }
                            });

            if (bestMove != null && !cancellationToken.IsCancellationRequested && requestId == LastEngineRequestId)
            {
                if (MovementExtension.ParseMovementFromUnifyString(bestMove.BestMovement, out Movement bestMovement))
                {
                    Logger.LogInformation("{} Received Best move {}", requestId, bestMove.BestMovement);

                    this.PopulateMovementWithContext(bestMovement);

                    if (immediateInvokeBestMove)
                    {
                        int count = queryCount;
                        CountdownEvent dispatchCountDownEvent = new CountdownEvent(1);

                        this.DispatchAction(() =>
                        {
                            this.ApplyMovement(bestMovement);
                            HistoryManager.InsertMovement(bestMovement);
                            SelectedChess = null;
                            Logger.LogDebug("{requestId}.{queryCount} Executed movement {}, turn {}", requestId, count,
                                            bestMovement,
                                            HistoryManager.ExecutionTurn);

                            EngineSuggestedMovements.Clear();
                            MovementIndicator.ClearMovement();

                            dispatchCountDownEvent.Signal();
                        });

                        // wait for the dispatch action to finished
                        try
                        {
                            dispatchCountDownEvent.Wait(cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                        }

                        // indicates to query engine again
                        recursiveQueryEngine = true;
                    }
                    else
                    {
                        this.DispatchAction(() => { MovementIndicator.SetMovement(bestMovement); });
                    }
                }
                else
                {
                    Logger.LogError($"Unable to parse best movement {bestMove.BestMovement}");
                }
            }
        } while (recursiveQueryEngine && !cancellationToken.IsCancellationRequested);

        IsEngineAutoMoving = false;

        Logger.LogDebug("End BlockingNotifyRefreshEngineFeedback {}", requestId);
    }

    private void DispatchAction(Action action)
    {
        if (Application.Current == null || Application.Current.MainPage == null)
        {
            Logger.LogError("Null application context");
            return;
        }

        Application.Current.MainPage.Dispatcher.Dispatch(action);
    }

    public override void SetChessAtPosition(Chess chess, Position position, out Chess removedChess)
    {
        base.SetChessAtPosition(chess, position, out removedChess);
        this.Matrix[position.Y][position.X].Chess = chess;
    }

    public void InitializeUcciEngine()
    {
        if (Engine != null) return;

        Logger.LogInformation("Initailzing engine...");
        Engine = ServiceProvider.GetService<UcciEngine>();

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var startUciFeedback = Engine.StartUci(cancellationTokenSource.Token);

        Engine.WaitToReady(cancellationTokenSource.Token);

        if (!startUciFeedback.ReceivedUcciOk || cancellationTokenSource.IsCancellationRequested)
        {
            Logger.LogError("Init ucci engine failed");
        }
        else
        {
            Logger.LogInformation($"init engine success");
        }
    }

    private ChessButton GetChessButtonAt(Position position)
    {
        return Matrix[position.Y][position.X];
    }

    public override void Reset()
    {
        base.Reset();
        HistoryManager.Reset();

        Iterator.IterateOverChessBoard(position =>
        {
            this.Matrix[position.Y][position.X].Chess = this.GetChessAtPosition(position);
        });

        this.SelectedChess = null;
    }

    public void MoveForward()
    {
        Movement movement = HistoryManager.MoveForward();
        if (movement != null)
        {
            this.ApplyMovement(movement);
        }
    }

    public void MoveBackward()
    {
        Movement movement = HistoryManager.MoveBackward();
        if (movement != null)
        {
            this.RevertMovement(movement);
        }
    }

    public void MoveToStart()
    {
        Movement movement;
        while ((movement = HistoryManager.MoveBackward()) != null)
        {
            this.RevertMovement(movement);
        }
    }

    public void MoveToEnd()
    {
        Movement movement;

        while ((movement = HistoryManager.MoveForward()) != null)
        {
            this.ApplyMovement(movement);
        }
    }

    private void SwitchInteractMode(EngineInteractiveMode mode)
    {
        if (this.InteractiveMode != mode)
        {
            Logger.LogInformation("Current interactive mode");

            StopEngineRunner();
            this.InteractiveMode = mode;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SwitchToAnalyzeModeColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SwitchToAutoModeColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SwitchToBlackAutoModeColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SwitchToRedAutoModeColor)));

            NotifyRefreshEngineFeedback();
        }
    }

    private void DisconnectEngine()
    {
        this.StopEngineRunner();
        Engine.Quit();

        Engine = null;
    }

    private void ReverseBoard()
    {
        Globals.Config[Keys.LayoutIsRotated] = (!Globals.Config.GetValue<bool>(Keys.LayoutIsRotated, false)).ToString();

        Iterator.IterateOverChessBoard(position => { GetChessButtonAt(position).Refresh(); });
        CursorSelector.Refresh();
        MovementIndicator?.Refresh();
    }

    public void InvokeBestMovement()
    {
        Movement movement = this.MovementIndicator?.Movement;
        if (movement != null)
        {
            Logger.LogInformation("[InvokeBestMovement] {}", movement);

            this.ApplyMovement(movement);
            HistoryManager.InsertMovement(movement);
        }
    }

    private ICommand WrapEngineInteractCommand(Action action, Func<object, bool> canExecute = null,
                    [CallerMemberName] string commandName = "")
    {
        return new Command<object>(obj =>
        {
            try
            {
                Logger.LogInformation($"Received command {commandName}");

                StopEngineRunner();
                action();
                NotifyRefreshEngineFeedback();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "MilkChess click Event error");
            }
        }, canExecute ?? (o => true));
    }

    private ICommand WrapClickCommand(Action action, Func<object, bool> canExecute = null,
                    [CallerMemberName] string commandName = "")
    {
        return new Command<object>(_ =>
        {
            try
            {
                Logger.LogInformation($"Received command {commandName}");
                action();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "MilkChess click Event error");
            }
        }, canExecute ?? (o => true));
    }

    private EventHandler WrapEventHandler(EventHandler handler, [CallerMemberName] string eventName = "")
    {
        return delegate(object sender, EventArgs args)
        {
            try
            {
                Logger.LogInformation("Received event {}", eventName);
                handler.Invoke(sender, args);
                Logger.LogInformation("Finished event {}", eventName);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Event error {}", eventName);
            }
        };
    }

    public class PredictMovementTrace
    {
        public int Score { get; set; }

        public int Depth { get; set; }

        public string Movements { get; set; }
    }

    private enum EngineInteractiveMode
    {
        AnalyzeMode,
        AutoMode,
        RedAutoMode,
        BlackAutoMode,
    }
}