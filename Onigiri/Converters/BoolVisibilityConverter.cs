using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Finalspace.Onigiri.Converters
{
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            Type t = value.GetType();
            if (typeof(bool).Equals(t))
            {
                bool b = (bool)value;
                return b ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (typeof(Visibility).Equals(t))
            {
                Visibility v = (Visibility)value;
                return (v == Visibility.Visible);
            }
            return (null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
