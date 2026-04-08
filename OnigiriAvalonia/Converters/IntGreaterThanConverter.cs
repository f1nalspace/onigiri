using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class IntGreaterThanConverter : IValueConverter
{
    public int Cutoff { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int integerValue)
            return integerValue >= Cutoff;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
