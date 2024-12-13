using System.Globalization;
using System.Windows.Data;

namespace FangJia.UI.Converters;

public class MainMenuListWidthConverter : IValueConverter
{

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double doubleValue ? doubleValue - 2 : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double doubleValue ? doubleValue + 2 : value;
    }
}
