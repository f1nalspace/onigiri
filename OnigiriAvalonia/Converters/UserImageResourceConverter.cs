using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Finalspace.Onigiri.Converters;

public class UserImageResourceConverter : IMultiValueConverter
{
    private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

    private static Bitmap CreateTextAvatar(string userName, int width, int height)
    {
        string text = userName[..Math.Min(userName.Length, 5)];
        var rtb = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        using (var ctx = rtb.CreateDrawingContext())
        {
            ctx.FillRectangle(Brushes.LightGray, new Rect(0, 0, width, height));
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                width * 0.4,
                Brushes.Black);
            ctx.DrawText(formattedText,
                new Point((width - formattedText.Width) / 2, (height - formattedText.Height) / 2));
        }
        return rtb;
    }

    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count < 2 ||
            values[0] is not string userName ||
            values[1] is not string imageName)
            return AvaloniaProperty.UnsetValue;

        string imageKey = $"{userName}@{imageName}";

        return _cache.GetOrAdd(imageKey, _ =>
        {
            string resourceUri = $"avares://OnigiriAvalonia/Resources/{imageName}";
            try
            {
                using var stream = AssetLoader.Open(new Uri(resourceUri));
                return new Bitmap(stream);
            }
            catch
            {
                // Resource not found — try user images path or generate avatar
                string userImagesPath = OnigiriPaths.UserImagesPath;
                if (!Directory.Exists(userImagesPath))
                    Directory.CreateDirectory(userImagesPath);

                string defaultImageFilePath = Path.Combine(userImagesPath, userName + ".png");
                if (File.Exists(defaultImageFilePath))
                    return new Bitmap(defaultImageFilePath);

                // Generate text avatar
                var avatar = CreateTextAvatar(userName, 64, 64);
                avatar.Save(defaultImageFilePath);
                return avatar;
            }
        });
    }
}
