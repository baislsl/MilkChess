using System.Globalization;

namespace MilkChess.Model;

public class Movement
{
    public bool IsPredict { get; set; }

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
        return $"{ActionCount}. {MovedChess?.Symbol} {From}->{To} {ChineseViewableString}";
    }

    public bool IsRevertibleMovement()
    {
        if (RemovedChess != null)
        {
            return false;
        }

        if (MovedChess is Pawn)
        {
            if (MovedChess.Color == Color.RED && From.Y == 3 && To.Y == 4)
            {
                return false;
            }

            if (MovedChess.Color == Color.BLACK && From.Y == 6 && To.Y == 5)
            {
                return false;
            }
        }

        return true;
    }
}

public class MovementColorConvertor : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
        {
            throw new ArgumentException($"values size {values.Length} not equal to 2");
        }

        if (values[0] is int && values[1] is int)
        {
            return (int)values[0] == (int)values[1]
                ? Colors.LightGray
                : Colors.LightBlue;
        }

        throw new ArgumentException($"invalid value {values[0]?.GetType()} - {values[1]?.GetType()}");
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}