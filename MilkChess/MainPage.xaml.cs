using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Layouts;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.Ucci;
using MilkChess.UserInterface;

namespace MilkChess
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger Logger;
        public ChessBoardViewModel ChessBoardViewModel { get; set; }
        private readonly HistoryManager HistoryManager;

        public MainPage(ChessBoardViewModel chessBoardViewModel,
                        HistoryManager historyManager,
                        ILogger<MainPage> logger)
        {
            ChessBoardViewModel = chessBoardViewModel;
            Logger = logger;
            HistoryManager = historyManager;

            InitializeComponent();

            BindingContext = chessBoardViewModel;

            InitChessBoardLayout();

            logger.LogInformation("Started");
        }

        public void InitChessBoardLayout()
        {
            Logger.LogInformation("InitChessBoardLayout");

            // background image
            Image backgroundImage = new Image()
            {
                            Source = UIConstants.ChessBoardBackgroundResource
            };
            AbsoluteLayout.SetLayoutBounds(backgroundImage,
                            new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(backgroundImage, AbsoluteLayoutFlags.PositionProportional);
            ChessBoardLayout.Add(backgroundImage);

            ChessBoardViewModel.AddToAbsoluteLayout(ChessBoardLayout);

            ExecutionTurnNotifier executionTurnNotifier = new ExecutionTurnNotifier(HistoryManager.ExecutionTurn);
            AbsoluteLayout.SetLayoutBounds(executionTurnNotifier,
                            new Rect(UIConstants.HorizontalOffset / 2.0, UIConstants.ChessBoardHeight / 2.0,
                                            AbsoluteLayout.AutoSize,
                                            AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(executionTurnNotifier, AbsoluteLayoutFlags.None);
            HistoryManager.AddActiveColorListener(delegate(object sender, EventArgs args)
            {
                executionTurnNotifier.ExecutionTurn = HistoryManager.ExecutionTurn;
            });
            ChessBoardLayout.Add(executionTurnNotifier);
        }

        // After multiple tries, I don't know how to bind within the list view to ChessBoardViewModel property
        public ICommand SwitchToSelectedMovementCommand => ChessBoardViewModel.SwitchToSelectedMovementCommand;

        public int CurrentHistoryManagerCursor => HistoryManager.GetCursor();
    }
}