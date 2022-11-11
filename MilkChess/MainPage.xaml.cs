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
        private readonly ChessBoardViewModel ChessBoardViewModel;
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

            ExecutionTurnNotifier executionTurnNotifier = new ExecutionTurnNotifier();
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
    }
}