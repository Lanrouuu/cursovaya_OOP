using Cursovaya.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cursovaya.Converters;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = value switch
        {
            UserRole.Admin => "RoleAdmin",
            UserRole.User => "RoleUser",
            ItemCondition.New => "ConditionNew",
            ItemCondition.Used => "ConditionUsed",
            ItemCondition.Damaged => "ConditionDamaged",
            ItemCondition.Other => "ConditionOther",
            AdvertisementStatus.Active => "StatusActive",
            AdvertisementStatus.Hidden => "StatusHidden",
            AdvertisementStatus.Blocked => "StatusBlocked",
            AdvertisementStatus.Deleted => "StatusDeleted",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(key)
            ? value?.ToString() ?? string.Empty
            : Application.Current.TryFindResource(key) as string ?? value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
