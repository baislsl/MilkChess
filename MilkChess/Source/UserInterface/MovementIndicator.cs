using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;
using MilkChess.Model;

namespace MilkChess.UserInterface;

public class MovementIndicator
{
    private readonly CursorSelector From;
    private readonly CursorSelector To;

    public Movement Movement { get; set; }

    public MovementIndicator()
    {
        this.From = new CursorSelector(new Position(0, 0), UIConstants.BaseMovementCursorResource);
        this.To = new CursorSelector(new Position(0, 0), UIConstants.BaseMovementCursorResource);

        this.From.IsEnabled = false;
        this.To.IsEnabled = false;
    }

    public void AddToAbsoluteLayout(AbsoluteLayout layout)
    {
        layout.Add(From);
        layout.Add(To);
    }

    public void SetMovement(Movement movement)
    {
        this.Movement = movement;

        if (movement != null)
        {
            From.Position = movement.From;
            To.Position = movement.To;
            From.IsEnabled = true;
            To.IsEnabled = true;
        }
        else
        {
            From.IsEnabled = false;
            To.IsEnabled = false;
        }
    }
}