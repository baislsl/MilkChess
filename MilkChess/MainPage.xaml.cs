using Microsoft.Extensions.Logging;
using Microsoft.Maui.Layouts;
using MilkChess.Helper;
using MilkChess.Source.Model;
using MilkChess.UserInterface;

namespace MilkChess
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger Logger;
        private readonly ChessBoardViewModel ChessBoardViewModel;
        private readonly ChessModel ChessModel;

        public MainPage(ChessBoardViewModel chessBoardViewModel,
            ChessModel chessModel,
            ILogger<MainPage> logger)
        {
            ChessBoardViewModel = chessBoardViewModel;
            ChessModel = chessModel;
            Logger = logger;

            InitializeComponent();
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
        }

        private void ResetCommand(object sender, EventArgs e)
        {
            ChessBoardViewModel.Reset();
        }

        private void MoveForwardCommand(object sender, EventArgs e)
        {
            ChessBoardViewModel.MoveForward();
        }

        private void MoveBackwardCommand(object sender, EventArgs e)
        {
            ChessBoardViewModel.MoveBackward();
        }

        private void MoveToStartCommand(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MoveToEndCommand(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}