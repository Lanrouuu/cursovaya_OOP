using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cursovaya.Converters;

public class CategoryNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = value?.ToString() ?? string.Empty;
        var key = name.Trim().ToLowerInvariant() switch
        {
            "электроника" or "electronics" => "CategoryElectronics",
            "одежда" or "clothes" or "clothing" => "CategoryClothes",
            "мебель" or "furniture" => "CategoryFurniture",
            "транспорт" or "transport" => "CategoryTransport",
            "недвижимость" or "real estate" => "CategoryRealEstate",
            "услуги" or "services" => "CategoryServices",
            "другое" or "other" => "CategoryOther",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(key)
            ? name
            : Application.Current.TryFindResource(key) as string ?? name;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
