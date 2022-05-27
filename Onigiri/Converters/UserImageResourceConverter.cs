using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Finalspace.Onigiri.Converters
{
    public class UserImageResourceConverter : IMultiValueConverter
    {
        private static readonly ConcurrentDictionary<string, ImageSource> _imageSourcesMap = new ConcurrentDictionary<string, ImageSource>();

        private static bool CanLoadResource(Uri uri)
        {
            try
            {
                Application.GetResourceStream(uri);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static BitmapSource CreateImage(string text, double width, double heigth)
        {
            // create WPF control
            Size size = new Size(width, heigth);

            Grid grid = new Grid() { Background = Brushes.LightGray };

            TextBlock text0 = new TextBlock();
            text0.VerticalAlignment = VerticalAlignment.Center;
            text0.HorizontalAlignment = HorizontalAlignment.Center;
            text0.TextWrapping = TextWrapping.NoWrap;
            text0.Text = text;
            text0.FontSize = width * 0.5f;
            text0.FontWeight = FontWeights.Bold;
            text0.Foreground = Brushes.Green;

            TextBlock text1 = new TextBlock();
            text1.VerticalAlignment = VerticalAlignment.Center;
            text1.HorizontalAlignment = HorizontalAlignment.Center;
            text1.TextWrapping = TextWrapping.NoWrap;
            text1.Text = text;
            text1.FontSize = width * 0.5f;
            text1.Padding = new Thickness(1, 2, 1, 2);
            text1.Foreground = Brushes.Black;

            grid.Children.Add(text0);
            grid.Children.Add(text1);

            // process layouting
            grid.Measure(size);
            grid.Arrange(new Rect(size));

            // Render control to an image
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(grid);
            return rtb;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is string userName &&
                values[1] is string imageName)
            {
                string imageKey = $"{userName}@{imageName}";

                bool hasImage = _imageSourcesMap.TryGetValue(imageKey, out ImageSource result);
                if (!hasImage)
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    string assemblyName = asm.GetName().Name;
                    string resourceName = $"Resources/{imageName}";
                    Uri uri = new Uri("pack://application:,,,/" + assemblyName + ";component/" + resourceName, UriKind.RelativeOrAbsolute);
                    if (false && CanLoadResource(uri))
                    {
                        result = new BitmapImage(uri);
                        result.Freeze();
                        _imageSourcesMap.AddOrUpdate(imageKey, result, (key, old) => result);
                    }
                    else
                    {
                        if (!Directory.Exists(OnigiriPaths.UserImagesPath))
                            Directory.CreateDirectory(OnigiriPaths.UserImagesPath);

                        string defaultImageFilePath = Path.Combine(OnigiriPaths.UserImagesPath, userName + ".png");
                        if (!File.Exists(defaultImageFilePath))
                        {
                            string text = userName.Substring(0, Math.Min(userName.Length, 5));
                            BitmapSource bitmap = CreateImage(text, 64, 64);
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));
                            using (FileStream stream = File.Create(defaultImageFilePath))
                                encoder.Save(stream);
                            result = bitmap;
                        }
                        else
                        {
                            BitmapImage bitmap = new BitmapImage();
                            using (FileStream fileStream = File.OpenRead(defaultImageFilePath))
                            {
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.StreamSource = fileStream;
                                bitmap.EndInit();
                                bitmap.Freeze();
                            }
                            result = bitmap;
                            _imageSourcesMap.AddOrUpdate(imageKey, result, (key, old) => result);
                        }
                    }
                }
                return result;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
