namespace MilkChess.Model;

public class Movement
{
    public Position From { get; set; }

    public Position To { get; set; }

    public Chess MovedChess { get; set; }

    public Chess RemovedChess { get; set; }

    public Color ExecutedColor => MovedChess.Color;

    public override string ToString()
    {
        return $"{MovedChess.Symbol} {From}->{To}";
    }

    public string GetStandardDescription()
    {
        // TODO;
        return this.ToString();
    }
}