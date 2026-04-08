using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Finalspace.Onigiri.Models;
using System;
using System.Globalization;
using System.IO;

namespace Finalspace.Onigiri.Converters;

public class AnimeImageConverter : IValueConverter
{
    private static Bitmap LoadImage(ReadOnlySpan<byte> imageData)
    {
        if (imageData.Length == 0) return null;
        using MemoryStream mem = new(imageData.Length);
        mem.Write(imageData);
        mem.Seek(0, SeekOrigin.Begin);
        return new Bitmap(mem);
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AnimeImage animeImage)
            return LoadImage(animeImage.Data.AsSpan());
        if (value is string pictureFile && File.Exists(pictureFile))
            return new Bitmap(pictureFile);
        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
