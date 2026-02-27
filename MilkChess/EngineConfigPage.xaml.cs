using MilkChess.UserInterface;

namespace MilkChess;

public partial class EngineConfigPage : ContentPage
{
    public EngineConfigPage(EngineConfigViewModel engineConfigViewModel)
    {
        InitializeComponent();
        BindingContext = engineConfigViewModel;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}