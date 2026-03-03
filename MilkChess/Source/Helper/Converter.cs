using System.Globalization;
using MilkChess.Model;
using MilkChess.UserInterface;

namespace MilkChess.Helper;

public class ExtendedConvertor : IValueConverter
{
    public static readonly IValueConverter MovementFromPositionLayoutBoundConverter
        = ExtendedConvertor.CreateConverter(new PositionLayoutBoundConverter(), m => (m as Movement)?.From);

    public static readonly IValueConverter MovementToPositionLayoutBoundConverter
        = ExtendedConvertor.CreateConverter(new PositionLayoutBoundConverter(), m => (m as Movement)?.To);

    public static readonly IValueConverter MovementFromPositionIsVisibleConverter
        = ExtendedConvertor.CreateConverter(new PositionIsVisibleConverter(), m => (m as Movement)?.From);

    public static readonly IValueConverter MovementToPositionIsVisibleConverter
        = ExtendedConvertor.CreateConverter(new PositionIsVisibleConverter(), m => (m as Movement)?.To);


    public IValueConverter BaseConverter { get; set; }
    public ValueUnwrapper[] Unwrappers { get; set; }

    public delegate object ValueUnwrapper(object value);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        foreach (var unwrapper in Unwrappers)
        {
            value = unwrapper(value);
        }

        return BaseConverter.Convert(value, targetType, parameter, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static IValueConverter CreateConverter(IValueConverter converter, params ValueUnwrapper[] unwrapper)
    {
        return new ExtendedConvertor()
        {
            BaseConverter = converter,
            Unwrappers = unwrapper,
        };
    }
}

public class ExecutorColorMovementMaskConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = value as Model.Color?;

        return color == Model.Color.BLACK
            ? UIConstants.BlackMovementCursorResource
            : UIConstants.RedMovementCursorResource;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

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

public class PositionLayoutBoundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var position = value as Position;

        return new Rect(
            position?.GetAbsoluteCoordinateX() ?? default,
            position?.GetAbsoluteCoordinateY() ?? default,
            AbsoluteLayout.AutoSize,
            AbsoluteLayout.AutoSize);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PositionIsVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var position = value as Position;
        return position != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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