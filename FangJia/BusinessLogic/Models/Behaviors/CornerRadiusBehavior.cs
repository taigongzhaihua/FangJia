using System.Windows;

namespace FangJia.BusinessLogic.Models.Behaviors;

public static class CornerRadiusBehavior
{
	public static readonly DependencyProperty CornerRadiusProperty =
		DependencyProperty.RegisterAttached("CornerRadius",
		                                    typeof(CornerRadius),
		                                    typeof(CornerRadiusBehavior),
		                                    new PropertyMetadata(new CornerRadius(0)));

	public static void SetCornerRadius(DependencyObject element, CornerRadius value)
	{
		element.SetValue(CornerRadiusProperty, value);
	}

	public static CornerRadius GetCornerRadius(DependencyObject element)
	{
		return (CornerRadius)element.GetValue(CornerRadiusProperty);
	}
}
