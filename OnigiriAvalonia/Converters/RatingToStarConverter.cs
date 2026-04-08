using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class RatingToStarConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

    public int StarIndex { get; set; }

    private static Bitmap LoadResource(string name)
    {
        return _cache.GetOrAdd(name, n =>
        {
            using var stream = AssetLoader.Open(new Uri($"avares://OnigiriAvalonia/Resources/{n}"));
            return new Bitmap(stream);
        });
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double rating)
            return AvaloniaProperty.UnsetValue;

        double lowerBound = StarIndex * 2.0;
        double upperBound = lowerBound + 1.0;

        if (rating > upperBound)
            return LoadResource("star2.png");
        if (rating > lowerBound)
            return LoadResource("star1.png");
        return LoadResource("star0.png");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
