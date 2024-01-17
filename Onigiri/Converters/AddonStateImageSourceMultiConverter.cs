using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Finalspace.Onigiri.Converters
{
    public class AddonStateImageSourceMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is string basePath &&
                values[1] is string username)
            {
                Uri uri = new Uri($"{basePath}{username}.png", UriKind.Relative); ;
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
