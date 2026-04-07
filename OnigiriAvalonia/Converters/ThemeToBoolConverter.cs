using Avalonia.Data.Converters;
using Finalspace.Onigiri.ViewModels;
using System;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class ThemeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MainTheme theme && parameter is MainTheme target)
            return theme == target;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
