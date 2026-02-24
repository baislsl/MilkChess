using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using MilkChess.UserInterface;

namespace MilkChess.Model;

public partial class IdleChess : ObservableObject
{
    [NotNull] [ObservableProperty] [NotifyPropertyChangedFor(nameof(ImageSource))]
    private Chess _chess;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImageSource))]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    private bool _isIdle;

    public int Index { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    private bool _isSelected;

    public string ImageSource
    {
        get
        {
            if (this.IsIdle)
            {
                return Chess.DrawableResource();
            }

            return null;
        }
    }

    public Microsoft.Maui.Graphics.Color BackgroundColor => this.IsIdle && this.IsSelected
        ? Colors.HotPink
        : Colors.Snow;


    public override string ToString()
    {
        return $"{Chess} IsIdle={IsIdle} Index={Index} IsSelected={IsSelected}";
    }
}