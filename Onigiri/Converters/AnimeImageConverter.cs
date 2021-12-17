using Finalspace.Onigiri.ViewModels;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Finalspace.Onigiri.Converters
{
    public class AnimeImageConverter : IMultiValueConverter
    {
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            BitmapImage image = new BitmapImage();
            using (MemoryStream mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
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

            if (values[0] is string)
            {
                string pictureFile = (string)values[0];
                if (!string.IsNullOrEmpty(pictureFile))
                    return new BitmapImage(new Uri(pictureFile));
            }
            else if (values[0] is ulong)
            {
                ulong aid = (ulong)values[0];
                MainViewModel vm = (MainViewModel)values[1];
                if (aid > 0 && vm != null)
                {
                    byte[] imageData = vm.CoreService.Cache.GetImageData(aid);
                    if (imageData != null)
                        return LoadImage(imageData);
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
