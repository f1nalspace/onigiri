using Finalspace.Onigiri.Models;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Finalspace.Onigiri.Converters
{
    public class AnimeImageConverter : IValueConverter
    {
        private static BitmapImage LoadImage(ReadOnlySpan<byte> imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            BitmapImage image = new BitmapImage();
            using (MemoryStream mem = new MemoryStream(imageData.Length))
            {
                mem.Write(imageData);
                mem.Seek(0, SeekOrigin.Begin);
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnimeImage animeImage)
                return LoadImage(animeImage.Data.AsSpan());
            else if (value is string pictureFile && File.Exists(pictureFile))
                return new BitmapImage(new Uri(pictureFile));
            else 
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
