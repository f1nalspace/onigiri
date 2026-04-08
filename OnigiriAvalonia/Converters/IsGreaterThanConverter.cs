using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class IsGreaterThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double val = System.Convert.ToDouble(value);
        double max = double.Parse(parameter as string, CultureInfo.InvariantCulture);
        return val > max;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
