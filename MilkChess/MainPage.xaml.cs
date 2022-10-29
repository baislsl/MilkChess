using Microsoft.Maui.Layouts;
using MilkChess.Source.Model;
using MilkChess.UserInterface;

namespace MilkChess
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        public ChessModel ChessModel { get; set; }

        public MainPage(BoardDrawable boardDrawable, ChessModel chessModel)
        {
            this.ChessModel = chessModel;

            InitializeComponent();
            
            this.Resources.Add("Board", boardDrawable);
            this.Resources.Add("ChessModel", chessModel);
        }
        
        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;


            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void ImageButton_OnClicked(object sender, EventArgs e)
        {
            CounterBtn.Text = $"Clicked {e.GetHashCode()}";
        }
    }
}