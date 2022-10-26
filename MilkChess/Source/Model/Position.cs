namespace MilkChess.Source.Model;

public class Position
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position()
    {
    }

    public Position(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}