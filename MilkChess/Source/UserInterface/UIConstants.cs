using Microsoft.Extensions.Configuration;
using MilkChess.Helper;
using MilkChess.Model;

namespace MilkChess.UserInterface;

public class UIConstants
{
    public const int ChessBoardHeight = 626;
    public const int ChessBoardWidth = 572;

    public const string ChessBoardBackgroundResource = "board.png";
    public const string CursorBackgroundResource = "mask.png";
    public const string BlackMovementCursorResource = "bmask.png";
    public const string RedMovementCursorResource = "rmask.png";

    public const int HorizontalOffset = 39;
    public const int VerticalOffset = 39;
    public const int ChessSize = 55;

    public const string ConnectEngineText = "ConnectEngine";
    public const string DisconnectEngineText = "DisconnectEngine";
}

internal static class UserInterfaceHelper
{
    public static int GetAbsoluteCoordinateX(this Position position)
    {
        return position.X * UIConstants.ChessSize + UIConstants.HorizontalOffset;
    }

    public static int GetAbsoluteCoordinateY(this Position position)
    {
        if (GlobalConfig.IsRotated)
        {
            return position.Y * UIConstants.ChessSize + UIConstants.VerticalOffset;
        }
        else
        {
            return (9 - position.Y) * UIConstants.ChessSize + UIConstants.VerticalOffset;
        }
    }

    public static string DrawableResource(this Chess chess)
    {
        string color = chess.Color == Model.Color.RED ? "r" : "b";
        string symbol = chess.Symbol.ToLower();
        return $"{color}{symbol}.png";
    }
}