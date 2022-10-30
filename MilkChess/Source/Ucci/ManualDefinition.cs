using MilkChess.Model;
using Color = MilkChess.Model.Color;
using static MilkChess.Ucci.Constants;

namespace MilkChess.Ucci;

public interface IChessManualTranslator
{
    void LoadFromStream(Stream file, IChessBoardHandler chessBoardHandler);

    void SaveToStream(Stream file, IChessBoardHandler chessBoardHandler);
}

public static class MovementExtension
{
    private static string ToUnifyString(this Position position)
    {
        return $"{XArixs[position.X]}{YArixs[position.Y]}";
    }

    private static string GetXCoordinateString(int x, Color color)
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

    public static string ToUnifyString(this Movement movement)
    {
        return $"{movement.From.ToUnifyString()}{movement.To.ToUnifyString()}";
    }

    // 马二进三
    // {Chess}{XCoordinate}{Action}{NewXCoordinate}
    // TODO: how to ensure in a dynamic movement?
    public static string ToChineseViewableString(this Movement movement, IChessBoardHandler chessBoardHandler)
    {
        string XCoordinate = GetXCoordinateString(movement.From.X, movement.ExecutedColor);
        string NewXCoordinate = GetXCoordinateString(movement.To.X, movement.ExecutedColor);

        // Action
        int dx = movement.To.X - movement.From.X,
                        dy = movement.To.Y - movement.To.Y;

        if (movement.MovedChess.Color == Color.BLACK)
        {
            dy = -dy;
        }

        string action = "";
        if (dy == 0)
        {
            action = ManualActionPan;
        }
        else
        {
            if (dx > 0)
            {
                action = ManualActionForward;
            }
            else
            {
                action = ManualActionBackward;
            }
        }

        // Chess
        string chessName = movement.MovedChess.ChineseName;
        List<int> inSameVerticalLine = new List<int>();
        for (int Y = 0; Y < ChessBoardHeight; Y++)
        {
            if (chessBoardHandler.GetChessAtPosition(new Position(movement.From.X, Y)) != null)
            {
                inSameVerticalLine.Add(Y);
            }
        }

        // TODO:
        if (inSameVerticalLine.Count > 0 && false)
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
        }
        
        return $"{chessName}{XCoordinate}{action}{NewXCoordinate}";
    }
}