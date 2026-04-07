using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class BoolToImageSourceConverter : IValueConverter
{
    public string TrueImage { get; set; }
    public string FalseImage { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is true;
        string uri = flag ? TrueImage : FalseImage;
        if (string.IsNullOrEmpty(uri))
            return null;
        using var stream = AssetLoader.Open(new Uri(uri));
        return new Bitmap(stream);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
