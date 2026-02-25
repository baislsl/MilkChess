using MilkChess.Model;
using Color = MilkChess.Model.Color;
using static MilkChess.Ucci.Constants;

namespace MilkChess.Ucci;

public interface IChessManualTranslator
{
    void LoadFromStream(Stream file, IChessBoardContext chessBoardContext);

    void SaveToStream(Stream file, IChessBoardContext chessBoardContext);
}

public static class MovementExtension
{
    private static string ToUnifyString(this Position position)
    {
        return $"{XArixs[position.X]}{YArixs[position.Y]}";
    }

    private static string GetChineseColorNumber(int x, Color color)
    {
        if (color == Color.RED)
        {
            return RedhorizontalCoordinate[x];
        }
        else
        {
            return BlackHorizontalCoordinate[x].ToString();
        }
    }

    private static string GetChineseXColorNumber(int x, Color color)
    {
        return color == Color.RED
            ? GetChineseColorNumber(8 - x, color)
            : GetChineseColorNumber(x, color);
    }

    public static string ToUnifyString(this Movement movement)
    {
        return $"{movement.From.ToUnifyString()}{movement.To.ToUnifyString()}";
    }

    public static bool ParsePositionFromUnifyString(string unifyString, out Position position)
    {
        position = null;
        if (unifyString.Length != 2) return false;

        int x = unifyString[0] - XArixs[0], y = unifyString[1] - YArixs[0];
        if (!Position.IsLegalXY(x, y)) return false;

        position = new Position(x, y);
        return true;
    }

    public static Movement CreateMovement(this IChessBoard chessBoard, Position from, Position to)
    {
        var movement = new Movement()
        {
            From = from,
            To = to,
        };

        chessBoard.PopulateMovement(movement);

        if (movement.MovedChess == null)
        {
            throw new ArgumentException("Moved chess is null");
        }

        if (!movement.MovedChess.IsValidPosition(movement.To))
        {
            throw new ArgumentException($"New position {movement.To} is invalid for chess {movement.MovedChess}");
        }

        return movement;
    }

    public static bool ParseUnifyStringMovement(this IChessBoard chessBoard, string unifyString, out Movement movement)
    {
        movement = null;

        if (unifyString?.Length != 4) return false;

        if (!ParsePositionFromUnifyString(unifyString.Substring(0, 2), out Position from))
        {
            return false;
        }

        if (!ParsePositionFromUnifyString(unifyString.Substring(2, 2), out Position to))
        {
            return false;
        }

        try
        {
            movement = chessBoard.CreateMovement(from, to);
        }
        catch (Exception)
        {
            return false;
        }


        return true;
    }

    /*
     * Movement at list contains from and to position
     */
    public static void PopulateMovement(this IChessBoard chessBoard, Movement movement)
    {
        movement.MovedChess = chessBoard.GetChessAtPosition(movement.From);
        movement.RemovedChess = chessBoard.GetChessAtPosition(movement.To);
        movement.ChineseViewableString = movement.ToChineseViewableString(chessBoard);

        if (chessBoard is IChessBoardContext chessBoardContext)
        {
            movement.ActionCount = chessBoardContext.HistoryManager.GetCursor() + 1;
        }
    }

    // 马二进三
    // {Chess}{XCoordinate}{Action}{NewCoordinate}
    public static string ToChineseViewableString(this Movement movement, IChessBoard chessBoard)
    {
        if (movement.MovedChess == null)
        {
            // TODO: fix
            return $"??{movement}";
        }

        try
        {
            return movement.ToChineseViewableStringInternal(chessBoard);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return $"??{movement}";
        }
    }

    // TODO: fix bug of ToChineseViewableStringInternal
    public static string ToChineseViewableStringInternal(this Movement movement, IChessBoard chessBoard)
    {
        string XCoordinate = GetChineseXColorNumber(movement.From.X, movement.ExecutedColor);

        // Action, NewCoordinate
        int dx = movement.To.X - movement.From.X,
            dy = movement.To.Y - movement.From.Y;

        if (movement.MovedChess.Color == Color.BLACK)
        {
            dy = -dy;
        }

        string action, newCoordinate;
        if (dy == 0)
        {
            action = ManualActionPan;
            newCoordinate = GetChineseXColorNumber(movement.To.X, movement.ExecutedColor);
        }
        else
        {
            if (dy > 0)
            {
                action = ManualActionForward;
            }
            else
            {
                action = ManualActionBackward;
            }

            if (dx == 0)
            {
                newCoordinate = GetChineseColorNumber(Math.Abs(dy) - 1, movement.ExecutedColor);
            }
            else
            {
                newCoordinate = GetChineseXColorNumber(movement.To.X, movement.ExecutedColor);
            }
        }

        // Chess
        string chessName = movement.MovedChess.ChineseName;

        List<int> inSameVerticalLine = new List<int>();
        for (int Y = 0; Y < ChessBoardHeight; Y++)
        {
            var chess = chessBoard.GetChessAtPosition(new Position(movement.From.X, Y));
            if (chess != null && chess.Number == movement.MovedChess.Number && chess.Color == movement.MovedChess.Color)
            {
                inSameVerticalLine.Add(Y);
            }
        }

        string header = null;
        // TODO: 兵的特殊情况暂且不管了，只处理最多两个同棋子的情况
        if (inSameVerticalLine.Count > 1)
        {
            int rank = -1;
            inSameVerticalLine.Sort();
            for (int i = 0; i < inSameVerticalLine.Count; i++)
            {
                if (inSameVerticalLine[i] == movement.From.Y)
                {
                    rank = i;
                    break;
                }
            }

            header = (rank == 0 && movement.ExecutedColor == Color.BLACK) ||
                     (rank == 1 && movement.ExecutedColor == Color.RED)
                ? PositionProportionalFirst
                : PositionProportionalLast;
        }

        return string.IsNullOrEmpty(header)
            ? $"{chessName}{XCoordinate}{action}{newCoordinate}"
            : $"{header}{chessName}{action}{newCoordinate}";
    }
}