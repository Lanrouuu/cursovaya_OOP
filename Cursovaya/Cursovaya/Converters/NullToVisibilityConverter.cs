using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cursovaya.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = value != null && !string.IsNullOrWhiteSpace(value.ToString());
        if (parameter?.ToString() == "Invert")
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
