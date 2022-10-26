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
    public const string King = nameof(King);
    public const string Advisor = nameof(Advisor);
    public const string Bishop = nameof(Bishop);
    public const string Knight = nameof(Knight);
    public const string Rook = nameof(Rook);
    public const string Cannon = nameof(Cannon);
    public const string Pawn = nameof(Pawn);
}