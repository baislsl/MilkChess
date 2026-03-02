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
    public const string LastMoveResource = "last_move.svg.png";

    public const double HorizontalOffset = 41.5;
    public const double VerticalOffset = 41;

    // TODO: remove extra offset
    public const double ChessImageExtraOffsetX = 2.0;
    public const double ChessImageExtraOffsetY = 1.0;

    // Chess picture Size is 56x56,
    // Chess size after removing the transparent board is 54x54
    public const int ChessSize = 54;

    public const string ConnectedEngineText = "ConnectEngine";
    public const string DisconnectedEngineText = "DisconnectEngine";

    public const string StopEngineText = "StopEngine";
    public const string EngineIdleStateText = "Idle";

    public const int BackgroundZIndex = (int)ChessBoardZIndex.Background;
    public const int ExecutionTurnZIndex = (int)ChessBoardZIndex.ExecutionTurn;
    public const int MovementIndicatorZIndex = (int)ChessBoardZIndex.MovementIndicator;
    public const int CursorSelectorZIndex = (int)ChessBoardZIndex.CursorSelector;
    public const int LastMoveIndicatorZIndex = (int)ChessBoardZIndex.LastMoveIndicator;
    public const int ChessZIndex = (int)ChessBoardZIndex.Chess;

    public const int IdleChessPanelWidth = 4*ChessSize;
    public const int IdleChessPanelHeight = 8*ChessSize;

    private enum ChessBoardZIndex
    {
        Background = 0,
        ExecutionTurn,
        MovementIndicator,
        CursorSelector,
        LastMoveIndicator,
        Chess,
    }
}

internal static class UserInterfaceHelper
{
    public static double GetAbsoluteCoordinateX(this Position position)
    {
        return position.X * UIConstants.ChessSize + UIConstants.HorizontalOffset;
    }

    public static double GetAbsoluteCoordinateY(this Position position)
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