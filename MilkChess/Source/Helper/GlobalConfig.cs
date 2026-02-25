using CommunityToolkit.Mvvm.ComponentModel;
using MilkChess.UserInterface;

namespace MilkChess.Helper;

public partial class GlobalConfig : ObservableObject
{
    public static GlobalConfig Instance { get; private set; } = new GlobalConfig();

    private GlobalConfig()
    {
    }

    public bool IsRotated { get; set; } = false;

    [ObservableProperty] private double _chessBoardScale = 1.0;

    public int ChessBoardVisualHeight => (int)(UIConstants.ChessBoardHeight * this.ChessBoardScale);

    public int ChessBoardVisualWidth => (int)(UIConstants.ChessBoardWidth * this.ChessBoardScale);

    partial void OnChessBoardScaleChanged(double value)
    {
        if (value < 0.65)
        {
            ChessBoardScale = 0.65;
        }

        if (value > 2.0)
        {
            ChessBoardScale = 2.0;
        }

        OnPropertyChanged(nameof(ChessBoardVisualWidth));
        OnPropertyChanged(nameof(ChessBoardVisualHeight));
    }
}