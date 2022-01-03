using Finalspace.Onigiri.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Finalspace.Onigiri.Converters
{
    public class ThemeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MainTheme activeTheme && parameter is MainTheme testTheme)
            {
                bool result = activeTheme == testTheme;
                return result;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
