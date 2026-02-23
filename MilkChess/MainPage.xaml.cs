using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Layouts;
using MilkChess.Model;
using MilkChess.UserInterface;

namespace MilkChess
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger Logger;
        public ChessBoardViewModel ChessBoardViewModel { get; set; }

        public MainPage(ChessBoardViewModel chessBoardViewModel,
                        ILogger<MainPage> logger)
        {
            ChessBoardViewModel = chessBoardViewModel;
            Logger = logger;

            InitializeComponent();

            BindingContext = chessBoardViewModel;

            InitChessBoardLayout();

            logger.LogInformation("Started");
        }

        public void InitChessBoardLayout()
        {
            Logger.LogInformation("InitChessBoardLayout");

            ChessBoardViewModel.AddToAbsoluteLayout(ChessBoardLayout);
        }
    }
}