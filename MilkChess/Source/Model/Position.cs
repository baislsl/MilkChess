using MilkChess.Ucci;

namespace MilkChess.Model;

public sealed class Position
{
    public readonly int X;

    public readonly int Y;

    public Position(int x, int y) : this(x, y, true)
    {
    }

    private Position(int x, int y, bool skipCheck)
    {
        if (!skipCheck && !IsLegalXY(x, y))
        {
            throw new ArgumentException($"{x} {y} invalid position");
        }

        this.X = x;
        this.Y = y;
    }

    public static bool operator ==(Position p1, Position p2)
    {
        if (ReferenceEquals(p1, p2))
            return true;
        if (ReferenceEquals(p1, null))
            return false;
        if (ReferenceEquals(p2, null))
            return false;

        return p1.Equals(p2);
    }

    public static bool operator !=(Position p1, Position p2)
    {
        return !(p1 == p2);
    }

    public override bool Equals(Object obj)
    {
        if (obj?.GetType() != typeof(Position))
        {
            return false;
        }
        else
        {
            Position p = (Position)obj;
            return p.X == this.X && p.Y == this.Y;
        }
    }

    public override int GetHashCode()
    {
        return (this.X << 2) ^ Y;
    }

    public override string ToString()
    {
        return $"{X}:{Y}";
    }

    private bool IsLegal()
    {
        return this.X >= 0 && this.X < Constants.ChessBoardWidth && this.Y >= 0 && this.Y < Constants.ChessBoardHeight;
    }

    public static bool IsLegalXY(int x, int y)
    {
        return new Position(x, y, true).IsLegal();
    }

    public bool IsLegalKingPosition(Color color)
    {
        return this.IsLegal()
               && this.X >= 3
               && this.X <= 5
               && (color != Color.BLACK || this.Y >= 7)
               && (color != Color.RED || this.Y <= 2);
    }

    public bool IsLegalAdvisorPosition(Color color)
    {
        return this.IsLegalKingPosition(color)
               && (color != Color.RED || (this.X + this.Y) % 2 == 1)
               && (color != Color.BLACK || (this.X + this.Y) % 2 == 0);
    }

    public bool IsLegalBiShopPosition(Color color)
    {
        if (!this.IsLegal()) return false;

        if (color == Color.BLACK)
        {
            return this.Y >= 5
                   && (this.X + this.Y) % 4 == 3;
        }
        else
        {
            return this.Y <= 4
                   && (this.X + this.Y) % 4 == 2;
        }
    }

    public bool InColorArea(Color color)
    {
        if (color == Color.RED)
        {
            return this.Y <= 4;
        }
        else
        {
            return this.Y >= 5;
        }
    }

    public static bool IsLegalKingPositionXY(Color color, int X, int Y)
    {
        return new Position(X, Y, true).IsLegalKingPosition(color);
    }

    public static bool IsLegalAdvisorPositionXY(Color color, int X, int Y)
    {
        return new Position(X, Y, true).IsLegalAdvisorPosition(color);
    }

    public static bool IsLegalBiShopPositionXY(Color color, int X, int Y)
    {
        return new Position(X, Y, true).IsLegalBiShopPosition(color);
    }

    public static bool InColorAreaXY(Color color, int X, int Y)
    {
        return new Position(X, Y).InColorArea(color);
    }
}