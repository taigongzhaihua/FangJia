using System.Collections;
using System.Windows;
using System.Windows.Controls;
using WPFDevelopers.Controls;

namespace FangJia.BusinessLogic.Models.Behaviors;

public static class SourceAttachedBehavior
{
	public static readonly DependencyProperty SourceProperty =
		DependencyProperty.RegisterAttached(
		                                    "Source",
		                                    typeof(object),
		                                    typeof(SourceAttachedBehavior),
		                                    new PropertyMetadata(null));

	public static void SetSource(UIElement element, object value)
	{
		element.SetValue(SourceProperty, value);
	}

	public static object GetSource(UIElement element)
	{
		return element.GetValue(SourceProperty);
	}
	public static readonly DependencyProperty SelectedItemsProperty =
		DependencyProperty.RegisterAttached(
		                                    "SelectedItems",
		                                    typeof(object),
		                                    typeof(SourceAttachedBehavior),
		                                    new FrameworkPropertyMetadata(null, 
		                                                                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
		                                                                  OnSelectedItemsChanged));

	private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not MultiSelectComboBox element) return;
		element.SelectedItems.Clear();
		foreach (var item in (e.NewValue as IList)!)
		{
			element.SelectedItems.Add(item);
		}
		element.SelectionChanged -= MultiSelectComboBox_SelectionChanged;
		element.SelectionChanged += MultiSelectComboBox_SelectionChanged;
	}

	private static void MultiSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is not MultiSelectComboBox element) return;
		var selectedItems = element.SelectedItems;
		SetSelectedItems(element, selectedItems);
	}

	public static void SetSelectedItems(UIElement element, object value)
	{
		element.SetValue(SelectedItemsProperty, value);
	}
	public static object GetSelectedItems(UIElement element)
	{
		return element.GetValue(SelectedItemsProperty);
	}

}
