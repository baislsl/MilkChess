using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.UserInterface;
using Color = MilkChess.Model.Color;
using static MilkChess.Ucci.Constants;

namespace MilkChess.Ucci;

public class ForsythEdwardsNotation
{
    public List<string> PiecePlacement { get; set; }
    public Color ActiveColor { get; set; }
    public int HalfmoveClock { get; set; }
    public int FullmoveNumber { get; set; }
    public List<string> Moves { get; set; }

    public ForsythEdwardsNotation()
    {
        this.PiecePlacement = new List<string>();
        this.HalfmoveClock = 0;
        this.FullmoveNumber = 1;
        this.Moves = new List<string>();
    }

    public static bool ParseString(string fen, out ForsythEdwardsNotation forsythEdwardsNotation)
    {
        forsythEdwardsNotation = null;

        string[] data = fen.Split(" ");

        if (data.Length < 6) return false;

        string[] piecePalcement = data[0].Split("/");
        if (piecePalcement.Length != 10) return false;

        if (data[1] != "w" && data[1] != "b") return false;

        if (data[2] != "-" || data[3] != "-") return false;

        if (!int.TryParse(data[4], out var halfmoveClock)) return false;
        if (!int.TryParse(data[5], out var fullmoveNumber)) return false;

        List<string> move = new List<string>();

        if (data.Length > 6)
        {
            if (data[6] != "moves") return false;
            for (int i = 7; i < data.Length; i++)
            {
                move.Add(data[i]);
            }
        }

        forsythEdwardsNotation = new ForsythEdwardsNotation()
        {
                        PiecePlacement = new List<string>(piecePalcement),
                        HalfmoveClock = halfmoveClock,
                        FullmoveNumber = fullmoveNumber,
                        Moves = move,
                        ActiveColor = data[1] == "w" ? Color.RED : Color.BLACK,
        };

        return true;
    }

    public string GetFenString()
    {
        string board = string.Join("/", PiecePlacement);
        string active = ActiveColor == Color.RED ? "w" : "b";

        string ret = $"{board} {active} - - {HalfmoveClock} {FullmoveNumber}";
        if (Moves.Count > 0)
        {
            ret += $" moves {string.Join(" ", Moves)}";
        }

        return ret;
    }
}

public class ForsythEdwardsNotationManualTranslator : IChessManualTranslator
{
    public void LoadFromStream(Stream file, IChessBoardContext chessBoardContext)
    {
        using StreamReader reader = new StreamReader(file);

        string fen = reader.ReadToEnd().Trim();

        if (ForsythEdwardsNotation.ParseString(fen, out ForsythEdwardsNotation forsythEdwardsNotation))
        {
            chessBoardContext.AdoptForsythEdwardsNotation(forsythEdwardsNotation);
        }
        else
        {
            throw new ArgumentException($"Invalid fen string {fen}");
        }
    }

    public void SaveToStream(Stream file, IChessBoardContext chessBoardContext)
    {
        using StreamWriter writer = new StreamWriter(file);
        writer.WriteLine(chessBoardContext.BuildForsythEdwardsNotation().GetFenString());
    }
}

/**
 * FEN definition
 */
public static class ForsythEdwardsNotationExtension
{
    private static Dictionary<string, Type> SymbolToChess = new Dictionary<string, Type>
    {
                    {ChessDefinition.Advisor.Symbol, typeof(Advisor)},
                    {ChessDefinition.Pawn.Symbol, typeof(Pawn)},
                    {ChessDefinition.Bishop.Symbol, typeof(Bishop)},
                    {ChessDefinition.King.Symbol, typeof(King)},
                    {ChessDefinition.Rook.Symbol, typeof(Rook)},
                    {ChessDefinition.Knight.Symbol, typeof(Knight)},
                    {ChessDefinition.Cannon.Symbol, typeof(Cannon)},
    };

    private static string ToFenSymbol(this Chess chess)
    {
        if (chess.Color == Color.RED)
        {
            return chess.Symbol.ToUpper();
        }
        else
        {
            return chess.Symbol.ToLower();
        }
    }

    public static bool AdoptForsythEdwardsNotation(this IChessBoardContext chessBoardContext,
                    string forsythEdwardsNotation)
    {
        if (ForsythEdwardsNotation.ParseString(forsythEdwardsNotation, out var fen))
        {
            return chessBoardContext.AdoptForsythEdwardsNotation(fen);
        }

        return false;
    }

    public static bool AdoptForsythEdwardsNotation(this IChessBoardContext chessBoardContext,
                    ForsythEdwardsNotation forsythEdwardsNotation)
    {
        for (int row = 0; row < ChessBoardHeight; row++)
        {
            int column = 0;
            string placement = forsythEdwardsNotation.PiecePlacement[row];
            foreach (var cc in placement)
            {
                if (cc > '0' && cc <= '9')
                {
                    for (var counter = '0'; counter < cc; counter++)
                    {
                        chessBoardContext.SetChessAtPosition(null, new Position(column++, ChessBoardHeight - 1 - row),
                                        out _);
                    }
                }
                else if (SymbolToChess.ContainsKey(cc.ToString().ToUpper()))
                {
                    Color color = Char.IsLower(cc) ? Color.BLACK : Color.RED;
                    Type type = SymbolToChess[cc.ToString().ToUpper()];
                    Chess chess = (Chess) type
                                    .GetConstructor(new Type[] {typeof(Color)})
                                    .Invoke(new object[] {color});

                    chessBoardContext.SetChessAtPosition(chess, new Position(column++, ChessBoardHeight - 1 - row),
                                    out _);
                }
                else
                {
                    return false;
                }
            }
        }

        chessBoardContext.HistoryManager.Reset();
        chessBoardContext.HistoryManager.ExecutionTurn = forsythEdwardsNotation.ActiveColor;

        return true;
    }

    public static ForsythEdwardsNotation BuildForsythEdwardsNotation(this IChessBoardContext chessBoardContext)
    {
        List<string> piecePlacement = new List<string>();
        for (int i = 0; i < ChessBoardHeight; i++)
        {
            string row = string.Empty;
            int j = 0;
            while (j < ChessBoardWidth)
            {
                int start = j;
                while (j < ChessBoardWidth &&
                       chessBoardContext.GetChessAtPosition(new Position(j, ChessBoardHeight - 1 - i)) == null)
                {
                    j++;
                }

                if (j > start)
                {
                    row += (j - start).ToString();
                }

                while (j < ChessBoardWidth &&
                       chessBoardContext.GetChessAtPosition(new Position(j, ChessBoardHeight - 1 - i)) != null)
                {
                    Chess chess = chessBoardContext.GetChessAtPosition(new Position(j, ChessBoardHeight - 1 - i));
                    row += chess.ToFenSymbol();
                    j++;
                }
            }

            piecePlacement.Add(row);
        }

        return new ForsythEdwardsNotation
        {
                        PiecePlacement = piecePlacement,
                        ActiveColor = chessBoardContext.HistoryManager.ExecutionTurn
        };
    }
}