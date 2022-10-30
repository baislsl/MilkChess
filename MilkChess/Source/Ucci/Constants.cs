using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChess.Ucci;

public static class Constants
{
    public const int ChessBoardWidth = 9;
    public const int ChessBoardHeight = 10;
}

public static class Command
{
    public const string Ucci = "ucci";

    public const string SetOption = "SetOption";

    public const string Position = "position";

    public const string Go = "go";

    public const string Quit = "Quit";
}

public static class ChessName
{
    public const string King = nameof(King);        // 将
    public const string Advisor = nameof(Advisor);  // 士
    public const string Bishop = nameof(Bishop);    // 象
    public const string Knight = nameof(Knight);    // 马
    public const string Rook = nameof(Rook);        // 车
    public const string Cannon = nameof(Cannon);    // 炮
    public const string Pawn = nameof(Pawn);        // 兵
}