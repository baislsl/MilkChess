using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Ucci;
using Color = Microsoft.Maui.Graphics.Color;

namespace MilkChess.UserInterface;

public partial class ChessBoardViewModel : ObservableObject, IChessBoardContext
{
    private readonly ChessModel ChessModel;

    public readonly List<List<ChessButton>> Matrix;

    public readonly ILogger Logger;

    private readonly IServiceProvider ServiceProvider;

    public readonly CursorSelector CursorSelector;

    public readonly MovementIndicator MovementIndicator;

    public readonly MilkChessConfig Config;

    private static readonly Color ActiveInteractiveModeColor = Colors.Blue;

    private static readonly Color InactiveInteractiveModeColor = Colors.CornflowerBlue;

    public HistoryManager HistoryManager { get; }

    public ChessBoardViewModel(
        IServiceProvider serviceProvider,
        ILogger<ChessBoardViewModel> logger,
        HistoryManager historyManager,
        ChessModel chessModel,
        IOptions<MilkChessConfig> milkChessConfig)
    {
        ChessModel = chessModel;
        Logger = logger;
        HistoryManager = historyManager;
        ServiceProvider = serviceProvider;
        Config = milkChessConfig.Value;

        Matrix = new List<List<ChessButton>>(ChessBoardHeight);
        for (int i = 0; i < ChessBoardHeight; i++)
        {
            Matrix.Add(new List<ChessButton>(new ChessButton[ChessBoardWidth]));
        }

        Iterator.IterateOverChessBoard(position =>
        {
            ChessButton chessButton = new ChessButton(null, position);

            // chessButton.Released += this.WrapEventHandler(this.OnChessSelected);
            chessButton.Command = this.WrapClickCommand(() => { this.OnChessSelected(chessButton, null); });
            Matrix[position.Y][position.X] = chessButton;
        });

        if (!LoadChessManual())
        {
            Logger.LogInformation("Use default chess position");
            // If init manual not loaded successfully, refresh with the default chess position
            Iterator.IterateOverChessBoard(position =>
            {
                this.SetChessAtPosition(this.GetChessAtPosition(position), position, out _);
            });
        }
        else
        {
            Logger.LogInformation("Use chess position from previous execution");
        }

        CursorSelector = new CursorSelector(this, null, UIConstants.CursorBackgroundResource);
        MovementIndicator = new MovementIndicator(this);
        EngineSuggestedMovements = new ObservableCollection<PredictMovementTrace>();
        this.ResetEngineModeColor();
    }

    public void AddToAbsoluteLayout(AbsoluteLayout layout)
    {
        MovementIndicator.AddToAbsoluteLayout(layout);

        Iterator.IterateOverChessBoard(position => { layout.Add(Matrix[position.Y][position.X]); });

        layout.Add(CursorSelector);
    }

    public void OnChessSelected(object sender, EventArgs args)
    {
        ChessButton clickedButton = (ChessButton)sender;
        Logger.LogInformation(
            $"Clicked {clickedButton.Position} {clickedButton.Chess}; Selected {CursorSelector.Position}, {CursorSelector.Chess}");

        if (CursorSelector.Chess != null
            && CursorSelector.Chess.Color == HistoryManager.ExecutionTurn
            && ChessModel.ValidateMovement(CursorSelector.Position, clickedButton.Position))
        {
            Movement movement = new Movement
            {
                MovedChess = CursorSelector.Chess,
                RemovedChess = clickedButton.Chess,
                From = CursorSelector.Position,
                To = clickedButton.Position,
                ActionCount = HistoryManager.GetCursor() + 1,
            };

            // 存下招法中文表示
            movement.ChineseViewableString =
                movement.ToChineseViewableString(this);

            Logger.LogInformation("Execute movement {}", movement);

            StopEngineRunner();

            this.ApplyMovement(movement);
            HistoryManager.InsertMovement(movement);

            CursorSelector.Position = clickedButton.Position;

            NotifyRefreshEngineFeedback();
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
    }

    public virtual void MoveChess(Position from, Position to, out Chess removedChess)
    {
        Chess chess = this.GetChessAtPosition(from);
        this.SetChessAtPosition(chess, to, out removedChess);
    }

    public virtual void Reset()
    {
        ChessModel.Reset();
        HistoryManager.Reset();

        Iterator.IterateOverChessBoard(position =>
        {
            this.Matrix[position.Y][position.X].Chess = this.GetChessAtPosition(position);
        });

        this.CursorSelector.Position = null;
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

    private void ResetEngineModeColor()
    {
        SwitchToAnalyzeModeColor = SwitchToRedAutoModeColor =
            SwitchToBlackAutoModeColor = SwitchToAutoModeColor = InactiveInteractiveModeColor;
    }


    private void SwitchInteractMode(EngineInteractiveMode mode)
    {
        if (this.InteractiveMode != mode)
        {
            Logger.LogInformation("Switch interactive mode {} -> {}", this.InteractiveMode, mode);

            StopEngineRunner();
            this.InteractiveMode = mode;
            this.ResetEngineModeColor();
            switch (this.InteractiveMode)
            {
                case EngineInteractiveMode.AnalyzeMode:
                    SwitchToAnalyzeModeColor = ActiveInteractiveModeColor;
                    break;
                case EngineInteractiveMode.AutoMode:
                    SwitchToAutoModeColor = ActiveInteractiveModeColor;
                    break;
                case EngineInteractiveMode.RedAutoMode:
                    SwitchToRedAutoModeColor = ActiveInteractiveModeColor;
                    break;
                case EngineInteractiveMode.BlackAutoMode:
                    SwitchToBlackAutoModeColor = ActiveInteractiveModeColor;
                    break;
                default:
                    throw new ArgumentException($"Unknown interactive mode {this.InteractiveMode}");
            }

            NotifyRefreshEngineFeedback();
        }
    }

    private string GetDataDirectory()
    {
        return Path.Combine(FileSystem.Current.AppDataDirectory,
            Config.PathDefinition.Root,
            Config.PathDefinition.DataPath);
    }

    private string GetChessManualFile()
    {
        return Path.Combine(GetDataDirectory(), "Chess.txt");
    }

    private void SaveChessManual()
    {
        string targetDirectory = GetDataDirectory();
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        string targetFile = GetChessManualFile();
        if (File.Exists(targetFile))
        {
            File.Delete(targetFile);
        }

        using FileStream outputStream = File.OpenWrite(targetFile);

        ForsythEdwardsNotationManualTranslator translator = new ForsythEdwardsNotationManualTranslator();
        translator.SaveToStream(outputStream, this);

        Logger.LogTrace("Saved chess manual to {}", targetFile);
    }

    private bool LoadChessManual()
    {
        string targetFile = GetChessManualFile();
        if (File.Exists(targetFile))
        {
            try
            {
                ForsythEdwardsNotationManualTranslator translator = new ForsythEdwardsNotationManualTranslator();
                using FileStream inputStream = File.OpenRead(targetFile);
                translator.LoadFromStream(inputStream, this);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "LoadChessManual exception");
            }
        }

        return false;
    }

    private void ReverseBoard()
    {
        GlobalConfig.IsRotated = !GlobalConfig.IsRotated;

        Iterator.IterateOverChessBoard(position => { Matrix[position.Y][position.X].Refresh(); });
        CursorSelector.Refresh();
        MovementIndicator?.Refresh();
    }

    private void InvokeBestMovement()
    {
        Movement movement = this.MovementIndicator?.Movement;
        if (movement != null)
        {
            Logger.LogInformation("[InvokeBestMovement] {}", movement);

            this.ApplyMovement(movement);
            HistoryManager.InsertMovement(movement);
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

    public class PredictMovementTrace
    {
        public int Score { get; set; }

        public int Depth { get; set; }

        public string Movements { get; set; }
    }
}