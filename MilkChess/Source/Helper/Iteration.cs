using MilkChess.Model;
using MilkChess.Protocol;

namespace MilkChess.Helper;

public static class Iterator
{
    public static void IterateOverChessBoard(Action<Position> action)
    {
        for (int i = 0; i < Constants.ChessBoardHeight; i++)
        {
            for (int j = 0; j < Constants.ChessBoardWidth; j++)
            {
                action(new Position(j, i));
            }
        }
    }
}