using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Finalspace.Onigiri.Converters;

public class BooleanLogicalOrMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is { Count: > 0 })
            return values.OfType<bool>().Any(v => v);
        return false;
    }
}
