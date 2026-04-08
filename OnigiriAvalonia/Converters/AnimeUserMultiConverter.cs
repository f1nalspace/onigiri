using Avalonia.Data.Converters;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Finalspace.Onigiri.Converters;

public class AnimeUserMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count >= 2 &&
            values[0] is Anime anime &&
            values[1] is string userName)
        {
            return new AnimeUserViewModel(anime, userName);
        }
        return null;
    }
}
