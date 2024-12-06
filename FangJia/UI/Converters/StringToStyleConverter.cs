using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FangJia.UI.Converters;

public class StringToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 确保 value 是 string 类型，并且不为空
        if (value is string styleKey && !string.IsNullOrEmpty(styleKey))
        {
            // 从资源字典中获取对应的 Style
            return (Application.Current.Resources[styleKey] as Style)!;
        }
        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null!; // 不支持反向转换
    }
}