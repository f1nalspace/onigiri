using Finalspace.Onigiri.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Finalspace.Onigiri.Converters
{
    public class AnimeUserMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && 
                values[0] is Anime anime &&
                values[1] is string userName)
            {
                return new Tuple<Anime, string>(anime, userName);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
