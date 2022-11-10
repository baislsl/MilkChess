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
        private readonly ChessModel ChessModel;
        private readonly HistoryManager HistoryManager;

        public MainPage(ChessBoardViewModel chessBoardViewModel,
                        ChessModel chessModel,
                        HistoryManager historyManager,
                        ILogger<MainPage> logger)
        {
            ChessBoardViewModel = chessBoardViewModel;
            ChessModel = chessModel;
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

            // chess
            Iterator.IterateOverChessBoard(position =>
            {
                Chess chess = ChessModel.GetChessAtPosition(position);
                ChessButton chessButton = new ChessButton(ChessModel, chess, position);

                chessButton.Clicked += ChessBoardViewModel.OnChessSelected;
                ChessBoardViewModel.Martix[position.Y][position.X] = chessButton;

                ChessBoardLayout.Add(chessButton);
            });

            ChessBoardLayout.Add(ChessBoardViewModel.CursorSelector);

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

        private void Button_OnClicked(object sender, EventArgs e)
        {
            ChessBoardViewModel.MoveBackwardCommand.Execute(sender);
        }
    }
}