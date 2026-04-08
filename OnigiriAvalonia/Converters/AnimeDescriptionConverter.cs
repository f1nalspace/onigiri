using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class AnimeDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            int maxLen = int.Parse(parameter as string ?? "0");
            if (maxLen > 0 && text.Length > maxLen)
                return $"{text[..maxLen]}...";
            return text;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
