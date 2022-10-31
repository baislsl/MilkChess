namespace MilkChess.Model;

public class Movement
{
    public Position From { get; set; }

    public Position To { get; set; }

    public Chess MovedChess { get; set; }

    public Chess RemovedChess { get; set; }

    public int ActionCount { get; set; }

    public Color ExecutedColor => MovedChess.Color;

    // 因为棋盘变化时动态确定棋子 前/后 比较麻烦，这里直接缓存招法的名称，比如前马二进三
    public string ChineseViewableString { get; set; }

    public Movement()
    {
    }

    public override string ToString()
    {
        return $"{ActionCount}. {MovedChess.Symbol} {From}->{To} {ChineseViewableString}";
    }
}