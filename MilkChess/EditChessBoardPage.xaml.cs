using CommunityToolkit.Mvvm.Input;
using MilkChess.Model;
using MilkChess.Protocol;
using MilkChess.UserInterface;

namespace MilkChess;

public record EditChessBoardResult(
    bool IsConfirmed,
    IChessBoard ChessBoard
);

public partial class EditChessBoardPage : ContentPage
{
    public readonly TaskCompletionSource<EditChessBoardResult> EditChessBoardResult = new();

    public readonly EditChessBoardViewModel EditChessBoardViewModel;

    public EditChessBoardPage(EditChessBoardViewModel editChessBoardViewModel)
    {
        this.EditChessBoardViewModel = editChessBoardViewModel;
        this.EditChessBoardViewModel.ParentPage = this;

        InitializeComponent();

        BindingContext = EditChessBoardViewModel;

        EditChessBoardViewModel.AddToAbsoluteLayout(this.ChessBoardAbsoluteLayout);
    }

    [RelayCommand]
    private async Task OnConfirm()
    {
        EditChessBoardResult.TrySetResult(new EditChessBoardResult(true, EditChessBoardViewModel));

        await Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task OnCancel()
    {
        EditChessBoardResult.TrySetResult(new EditChessBoardResult(false, null));

        await Navigation.PopModalAsync();
    }
}