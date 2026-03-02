using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls;
using MilkChess.Model;
using Color = Microsoft.Maui.Graphics.Color;
using System.Windows.Input;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
#endif

#if MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace MilkChess.UserInterface;

public class ChessButton : ImageButton
{
    private Chess _chess;

    private bool rightClickHooked = false;

    public readonly Position Position;

    public Chess Chess
    {
        get => _chess;
        set
        {
            this._chess = value;
            this.Source = value?.DrawableResource();
        }
    }

    public static readonly BindableProperty RightClickCommandProperty =
        BindableProperty.Create(
            nameof(RightClickCommand),
            typeof(ICommand),
            typeof(ChessButton));

    public ICommand RightClickCommand
    {
        get => (ICommand)GetValue(RightClickCommandProperty);
        set => SetValue(RightClickCommandProperty, value);
    }

    public ChessButton(Chess chess, Position position)
    {
        this.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        this.Position = position;
        this.Chess = chess;
        this.ZIndex = UIConstants.ChessZIndex;

        this.HandlerChanged += OnHandlerChanged;

        Refresh();
    }

    private void OnHandlerChanged(object sender, EventArgs e)
    {
        if (rightClickHooked || Handler?.PlatformView == null)
            return;

#if WINDOWS
        if (Handler.PlatformView is UIElement element)
        {
            element.PointerPressed += OnWindowsPointerPressed;
            rightClickHooked = true;
        }
#endif

#if MACCATALYST
        if (Handler.PlatformView is UIView view)
        {
            var interaction = new UIContextMenuInteraction(
                new RightClickContextDelegate(this));

            view.AddInteraction(interaction);
            rightClickHooked = true;
        }
#endif
    }

#if WINDOWS
    private void OnWindowsPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint((UIElement)sender);

        if (point.Properties.IsRightButtonPressed)
        {
            if (RightClickCommand?.CanExecute(this.CommandParameter) == true)
                RightClickCommand.Execute(this.CommandParameter);

            e.Handled = true;
        }
    }
#endif

#if MACCATALYST
    private class RightClickContextDelegate : UIContextMenuInteractionDelegate
    {
        private readonly ChessButton button;

        public RightClickContextDelegate(ChessButton button)
        {
            this.button = button;
        }

        public override UIContextMenuConfiguration GetConfigurationForMenu(
            UIContextMenuInteraction interaction,
            CGPoint location)
        {
            if (button.RightClickCommand?.CanExecute(button.CommandParameter) == true)
            {
                button.RightClickCommand.Execute(button.CommandParameter);
            }

            return null;
        }
    }
#endif


    public void Refresh()
    {
        this.Source = _chess?.DrawableResource();
        AbsoluteLayout.SetLayoutBounds(this, new Rect(
            Position.GetAbsoluteCoordinateX() + UIConstants.ChessImageExtraOffsetX,
            Position.GetAbsoluteCoordinateY() + UIConstants.ChessImageExtraOffsetY,
            UIConstants.ChessSize,
            UIConstants.ChessSize));
        AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
    }
}