using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class ReferenceEqualsToBooleanMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count == 2)
            return object.Equals(values[0], values[1]);
        return false;
    }
}
