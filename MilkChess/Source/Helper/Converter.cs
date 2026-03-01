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

public class StringListToStringConverter : IValueConverter
{
    // List<string> -> string
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> list)
            return string.Join(" ", list);

        return string.Empty;
    }

    // string -> List<string>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            return s
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        return new List<string>();
    }
}

public class EqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}