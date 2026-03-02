using MilkChess.Helper;
using MilkChess.UserInterface;

namespace MilkChess;

public partial class PredictTracePreviewPage : ContentPage
{
    public PredictTracePreviewViewModel ViewModel { get; }

    public PredictTracePreviewPage(PredictTracePreviewViewModel viewModel)
    {
        this.ViewModel = viewModel;

        InitializeComponent();

        BindingContext = viewModel;

        viewModel.AddToAbsoluteLayout(this.ChessBoardAbsoluteLayout);

    }

}