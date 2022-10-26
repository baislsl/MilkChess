namespace MilkChess.Ucci;

public class ChessDefinition
{
    public static readonly ChessDefinition King = new ChessDefinition(ChessName.King, "K", 1);
    public static readonly ChessDefinition Advisor = new ChessDefinition(ChessName.Advisor, "A", 2);
    public static readonly ChessDefinition Bishop = new ChessDefinition(ChessName.Bishop, "B", 3);
    public static readonly ChessDefinition Knight = new ChessDefinition(ChessName.Knight, "N", 4);
    public static readonly ChessDefinition Rook = new ChessDefinition(ChessName.Rook, "R", 5);
    public static readonly ChessDefinition Cannon = new ChessDefinition(ChessName.Cannon, "C", 6);
    public static readonly ChessDefinition Pawn = new ChessDefinition(ChessName.Pawn, "P", 7);

    public string Name { get; }

    public string Symbol { get; }

    public int Number { get; }

    private ChessDefinition(string name, string symbol, int number)
    {
        this.Name = name;
        this.Symbol = symbol;
        this.Number = number;
    }
}