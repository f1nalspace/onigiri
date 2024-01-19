using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Finalspace.Onigiri.Converters
{
    public class ReferenceEqualsToBooleanMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                object value0 = values[0];
                object value1 = values[1];
                return object.Equals(value0, value1);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
