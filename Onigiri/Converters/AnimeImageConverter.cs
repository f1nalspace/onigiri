using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.ViewModels;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Finalspace.Onigiri.Converters
{
    public class AnimeImageConverter : IMultiValueConverter
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

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return DependencyProperty.UnsetValue;

            if (!(values[1] is MainViewModel mainViewModel))
                return DependencyProperty.UnsetValue;

            if (values[0] is string)
            {
                string pictureFile = (string)values[0];
                if (!string.IsNullOrEmpty(pictureFile))
                    return new BitmapImage(new Uri(pictureFile));
            }
            else if (values[0] is AnimeImage animeIage)
                return LoadImage(animeIage.Data.AsSpan());
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
