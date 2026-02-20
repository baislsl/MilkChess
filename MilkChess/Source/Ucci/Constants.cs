namespace MilkChess.Ucci;

public static class Constants
{
    public const int ChessBoardWidth = 9;
    public const int ChessBoardHeight = 10;

    public const string XArixs = "abcdefghi";

    public const string YArixs = "0123456789";

    public static List<string> RedhorizontalCoordinate = new List<string>
    {
        "一", "二", "三", "四", "五", "六", "七", "八", "九",
    };

    public const string BlackHorizontalCoordinate = "123456789";

    public const string ManualActionForward = "进";

    public const string ManualActionBackward = "退";

    public const string ManualActionPan = "平";

    public const string PositionProportionalFirst = "前";

    public const string PositionProportionalLast = "后";
}

public static class Command
{
    public const string Ucci = "uci";

    public const string IsReady = "isready";

    public const string SetOption = "setOption";

    public const string Position = "position";

    public const string Go = "go";

    public const string Stop = "stop";

    public const string Quit = "quit";
}

public static class ChessName
{
    public const string King = nameof(King); // 将
    public const string Advisor = nameof(Advisor); // 士
    public const string Bishop = nameof(Bishop); // 象
    public const string Knight = nameof(Knight); // 马
    public const string Rook = nameof(Rook); // 车
    public const string Cannon = nameof(Cannon); // 炮
    public const string Pawn = nameof(Pawn); // 兵
}