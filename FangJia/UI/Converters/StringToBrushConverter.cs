using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FangJia.UI.Converters;

public class StringToBrushConverter : IValueConverter
{
	/// <summary>
	/// 将字符串转换为 Color
	/// </summary>
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{

		var colorString = value as string;
		if (string.IsNullOrWhiteSpace(colorString)) return Colors.Transparent;
		var converter = new BrushConverter();
		var brush     = converter.ConvertFromString(colorString) as Brush;
		return brush ?? Brushes.Transparent;

	}

	/// <summary>
	/// 将 Color 转换回字符串
	/// </summary>
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not Brush brush) return string.Empty; // 默认返回空字符串
		// 将 Brush 转换为 SolidColorBrush
		if (brush is not SolidColorBrush solidColorBrush) return string.Empty; // 默认返回空字符串
		// 获取颜色
		var color = solidColorBrush.Color;
		// 转换回十六进制颜色字符串
		return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";


	}
}
