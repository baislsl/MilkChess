namespace MilkChess.Source.Model;

public class Movement
{
    public Position From { get; set; }

    public Position To { get; set; }

    public Chess MoveChess { get; set; }

    public Chess RemoveChess { get; set; }
}