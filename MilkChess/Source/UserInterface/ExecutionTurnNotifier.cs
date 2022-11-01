using Microsoft.Maui.Controls.Shapes;
using Color = MilkChess.Model.Color;

namespace MilkChess.UserInterface;

public class ExecutionTurnNotifier : BoxView
{
    public Color _color;

    public Color ExecutionTurn
    {
        get => _color;
        set
        {
            this._color = value;
            this.Color = value == Model.Color.RED ? Colors.Red : Colors.Black;
        }
    }

    public ExecutionTurnNotifier()
    {
        WidthRequest = 20;
        HeightRequest = 20;
        CornerRadius = 10;
        ExecutionTurn = Model.Color.RED;
    }
}