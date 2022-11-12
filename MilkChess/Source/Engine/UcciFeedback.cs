namespace MilkChess.Engine;

public enum FeedbackCommand
{
    Id,
    Option,
    UciOk,
    ReadyOk,
    Info,
    BestMove,
    NoBestMove,
    PopHash,
    Bye,
}

public enum SubInfoCommand
{
    Time,
    Depth,
    CurrMove,
    Message,
}

public class UcciFeedback
{
    public string RawCommand;
    public FeedbackCommand Command;

    #region fields for Id

    public string IdName;
    public string IdValue;

    #endregion

    #region fields for Option

    #endregion


    #region fields for Info

    public List<string> InfoPv;
    public Dictionary<string, string> InfoDict;

    #endregion

    #region BestMove

    public string BestMoveBestMovement;
    public string BestMovePonder;
    public bool BestMoveDraw;
    public bool BestMoveResign;

    #endregion


    public override string ToString()
    {
        return this.RawCommand;
    }

    public static bool TryParse(string s, out UcciFeedback feedback)
    {
        feedback = new UcciFeedback();

        s = s.Trim();
        var list = s.Split(' ');

        if (list.Length <= 0)
        {
            return false;
        }

        if (!Enum.TryParse(list[0], true, out feedback.Command))
        {
            return false;
        }

        int idx = 1;
        switch (feedback.Command)
        {
            case FeedbackCommand.Id:
                if (list.Length < 3) return false;
                feedback.IdName = list[1];
                feedback.IdValue = string.Join(' ', list, 2);
                break;
            case FeedbackCommand.Option:
                if (list.Length < 4) return false;
                // TODO: parse option
                break;
            case FeedbackCommand.UciOk:
                break;
            case FeedbackCommand.ReadyOk:
                break;
            case FeedbackCommand.Info:
                feedback.InfoDict = new Dictionary<string, string>();
                feedback.InfoPv = new List<string>();
                while (idx < list.Length)
                {
                    if (list[idx] != "pv")
                    {
                        if (list[idx] == "lowerbound" || list[idx] == "upperbound") idx++; // adapt for GGchess and XuanFeng
                        if (idx + 1 >= list.Length) return false;
                        feedback.InfoDict[list[idx]] = list[idx + 1];
                        idx += 2;
                    }
                    else
                    {
                        feedback.InfoPv = list.Skip(idx + 1).ToList();
                        break;
                    }
                }

                break;
            case FeedbackCommand.BestMove:
                if (list.Length < 2) return false;
                feedback.BestMoveBestMovement = list[idx++];
                while (idx < list.Length)
                {
                    string action = list[idx++];
                    switch (action)
                    {
                        case "ponder":
                            if (idx >= list.Length) return false;
                            feedback.BestMovePonder = list[idx++];
                            break;
                        case "draw":
                            feedback.BestMoveDraw = true;
                            break;
                        case "resign":
                            feedback.BestMoveResign = true;
                            break;
                        default:
                            return false;
                    }
                }

                break;
            case FeedbackCommand.NoBestMove:
                break;

            case FeedbackCommand.PopHash:
                break;
            case FeedbackCommand.Bye:
                break;
        }

        return true;
    }
}