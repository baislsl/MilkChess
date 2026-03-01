using System.Globalization;

namespace MilkChess.Helper;

public class MaxDoubleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var a = values[0] as double? ?? 0;
        var b = values[1] as double? ?? 0;
        return Math.Max(a, b);
    }


    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}