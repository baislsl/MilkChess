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

    public const string ConnectedEngineText = "ConnectEngine";
    public const string DisconnectedEngineText = "DisconnectEngine";

    public const string StopEngineText = "StopEngine";
    public const string EngineIdleStateText = "Idle";

    public const int BackgroundZIndex = (int)ChessBoardZIndex.Background;
    public const int ExecutionTurnZIndex = (int)ChessBoardZIndex.ExecutionTurn;
    public const int MovementIndicatorZIndex = (int)ChessBoardZIndex.MovementIndicator;
    public const int CursorSelectorZIndex = (int)ChessBoardZIndex.CursorSelector;
    public const int ChessZIndex = (int)ChessBoardZIndex.Chess;

    public const int IdleChessPanelWidth = 4*ChessSize;
    public const int IdleChessPanelHeight = 8*ChessSize;

    private enum ChessBoardZIndex
    {
        Background = 0,
        ExecutionTurn,
        MovementIndicator,
        CursorSelector,
        Chess,
    }
}

internal static class UserInterfaceHelper
{
    public static int GetAbsoluteCoordinateX(this Position position)
    {
        return position.X * UIConstants.ChessSize + UIConstants.HorizontalOffset;
    }

    public static int GetAbsoluteCoordinateY(this Position position)
    {
        if (GlobalConfig.Instance.IsRotated)
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