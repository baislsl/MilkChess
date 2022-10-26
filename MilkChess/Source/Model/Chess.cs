using MilkChess.Ucci;

namespace MilkChess.Source.Model;

public enum Color
{
    RED,
    BLACK
}

public abstract class Chess : ICloneable
{
    public abstract ChessDefinition ChessDefinition { get; }

    public string Name => ChessDefinition.Name;

    public string Symbol => ChessDefinition.Symbol;

    public int Number => ChessDefinition.Number;

    public Color Color { get; set; }

    public Chess()
    {
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