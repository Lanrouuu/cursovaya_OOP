using Cursovaya.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cursovaya.Converters;

public class RoleToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var expectedRole = parameter?.ToString();
        var isVisible = value is UserRole role && role.ToString() == expectedRole;
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
