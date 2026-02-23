using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using MilkChess.Model;

namespace MilkChess.UserInterface;

public partial class CursorSelector : ObservableObject
{
    private IChessBoard ChessBoard { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LayoutBound))]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private Position _position;

    public Rect LayoutBound => new Rect(
        Position?.GetAbsoluteCoordinateX() ?? default,
        Position?.GetAbsoluteCoordinateY() ?? default,
        AbsoluteLayout.AutoSize,
        AbsoluteLayout.AutoSize);

    public bool IsVisible => this.Position != null;

    public Chess Chess => this.ChessBoard.GetChessAtPosition(this.Position);

    public CursorSelector(IChessBoard chessBoard, Position position)
    {
        this.ChessBoard = chessBoard;
        this.Position = position;
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(LayoutBound));
    }
}