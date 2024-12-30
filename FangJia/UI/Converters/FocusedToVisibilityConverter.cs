using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FangJia.UI.Converters;

public class FocusedToVisibilityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		return values.Any(value => value is true) ? Visibility.Visible : Visibility.Collapsed;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		return [value, value];
	}
}
