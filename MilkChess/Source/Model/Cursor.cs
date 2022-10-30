namespace MilkChess.Model;

public class Cursor
{
    public Position Position { get; set; }

    public Cursor(Position position)
    {
        this.Position = position;
    }
}