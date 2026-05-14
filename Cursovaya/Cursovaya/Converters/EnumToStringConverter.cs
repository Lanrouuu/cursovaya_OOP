using Cursovaya.Models;
using System.Globalization;
using System.Windows.Data;

namespace Cursovaya.Converters;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isEnglish = CultureInfo.CurrentUICulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        if (isEnglish)
        {
            return value switch
            {
                UserRole.Admin => "Admin",
                UserRole.User => "User",
                ItemCondition.New => "New",
                ItemCondition.Used => "Used",
                ItemCondition.Damaged => "Damaged",
                ItemCondition.Other => "Other",
                AdvertisementStatus.Active => "Active",
                AdvertisementStatus.Hidden => "Hidden",
                AdvertisementStatus.Blocked => "Blocked",
                AdvertisementStatus.Deleted => "Deleted",
                _ => value?.ToString() ?? string.Empty
            };
        }

        return value switch
        {
            UserRole.Admin => "Администратор",
            UserRole.User => "Пользователь",
            ItemCondition.New => "Новое",
            ItemCondition.Used => "Б/у",
            ItemCondition.Damaged => "Повреждено",
            ItemCondition.Other => "Другое",
            AdvertisementStatus.Active => "Активно",
            AdvertisementStatus.Hidden => "Скрыто",
            AdvertisementStatus.Blocked => "Заблокировано",
            AdvertisementStatus.Deleted => "Удалено",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
