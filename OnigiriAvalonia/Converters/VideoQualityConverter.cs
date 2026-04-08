using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class VideoQualityConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

    private static Bitmap LoadResource(string name)
    {
        return _cache.GetOrAdd(name, n =>
        {
            string uri = $"avares://OnigiriAvalonia/Resources/{n}";
            using var stream = AssetLoader.Open(new Uri(uri));
            return new Bitmap(stream);
        });
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int height)
            return AvaloniaProperty.UnsetValue;

        return height switch
        {
            >= 2160 => LoadResource("v_2160p.png"),
            >= 1440 => LoadResource("v_1440p.png"),
            >= 1080 => LoadResource("v_1080p.png"),
            >= 720 => LoadResource("v_720p.png"),
            _ => LoadResource("v_576p.png"),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
