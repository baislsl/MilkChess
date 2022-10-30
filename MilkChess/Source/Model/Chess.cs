using MilkChess.Ucci;

namespace MilkChess.Model;

public class ChessDefinition
{
    public static readonly ChessDefinition King = new ChessDefinition(ChessName.King, "K", 1, "帅", "将");
    public static readonly ChessDefinition Advisor = new ChessDefinition(ChessName.Advisor, "A", 2, "仕", "士");
    public static readonly ChessDefinition Bishop = new ChessDefinition(ChessName.Bishop, "B", 3, "象", "相");
    public static readonly ChessDefinition Knight = new ChessDefinition(ChessName.Knight, "N", 4, "马", "马");
    public static readonly ChessDefinition Rook = new ChessDefinition(ChessName.Rook, "R", 5, "车", "车");
    public static readonly ChessDefinition Cannon = new ChessDefinition(ChessName.Cannon, "C", 6, "炮", "炮");
    public static readonly ChessDefinition Pawn = new ChessDefinition(ChessName.Pawn, "P", 7, "兵", "兵");

    public string Name { get; }

    public string Symbol { get; }

    public int Number { get; }

    public string RName { get; set; }

    public string BName { get; set; }

    private ChessDefinition(string name, string symbol, int number, string rName, string bName)
    {
        this.Name = name;
        this.Symbol = symbol;
        this.Number = number;
    }
}

public enum Color
{
    RED,
    BLACK
}

public static class ColorExtension
{
    public static Color Reverse(this Color color)
    {
        return color == Color.RED ? Color.BLACK : Color.RED;
    }
}

public abstract class Chess : ICloneable
{
    public abstract ChessDefinition ChessDefinition { get; }

    public string Name => ChessDefinition.Name;

    public string Symbol => ChessDefinition.Symbol;

    public int Number => ChessDefinition.Number;

    public Color Color { get; set; }

    public string ChineseName
    {
        get { return this.Color == Color.RED ? ChessDefinition.RName : ChessDefinition.BName; }
    }

    public Chess()
    {
    }

    public override string ToString()
    {
        return this.Name;
    }

    public Chess(Color color) : this()
    {
        this.Color = color;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

/**
 * 象
 */
public class Bishop : Chess
{
    public override ChessDefinition ChessDefinition => ChessDefinition.Bishop;

    public Bishop(Color color)
                    : base(color)
    {
    }
}

/**
 * 炮
 */
public class Cannon : Chess
{
    public override ChessDefinition ChessDefinition => ChessDefinition.Cannon;

    public Cannon(Color color) : base(color)
    {
    }
}

// 将/帅
public class King : Chess
{
    public override ChessDefinition ChessDefinition => ChessDefinition.King;

    public King(Color color) : base(color)
    {
    }
}

// 马
public class Knight : Chess
{
    public override ChessDefinition ChessDefinition => ChessDefinition.Knight;

    public Knight(Color color) : base(color)
    {
    }
}

// 士
public class Advisor : Chess
{
    public override ChessDefinition ChessDefinition => ChessDefinition.Advisor;

    public Advisor(Color color) : base(color)
    {
    }
}

// 兵
public class Pawn : Chess
{
    public override ChessDefinition ChessDefinition => ChessDefinition.Pawn;

    public Pawn(Color color) : base(color)
    {
    }
}

// 车
public class Rook : Chess
{
    public override ChessDefinition ChessDefinition => ChessDefinition.Rook;

    public Rook(Color color) : base(color)
    {
    }
}