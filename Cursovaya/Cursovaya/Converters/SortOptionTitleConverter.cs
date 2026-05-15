using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cursovaya.Converters;

public class SortOptionTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = value?.ToString() switch
        {
            "date_desc" => "SortNewest",
            "price_asc" => "SortPriceAsc",
            "price_desc" => "SortPriceDesc",
            "title" => "SortTitle",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(key)
            ? value?.ToString() ?? string.Empty
            : Application.Current.TryFindResource(key) as string ?? key;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
