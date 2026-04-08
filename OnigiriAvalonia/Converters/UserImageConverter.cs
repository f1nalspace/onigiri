using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class UserImageConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string username || string.IsNullOrEmpty(username))
            return AvaloniaProperty.UnsetValue;

        return _cache.GetOrAdd(username, name =>
        {
            string resourceUri = $"avares://OnigiriAvalonia/Resources/{name}.png";
            try
            {
                using var stream = AssetLoader.Open(new Uri(resourceUri));
                return new Bitmap(stream);
            }
            catch
            {
                return null;
            }
        });
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
