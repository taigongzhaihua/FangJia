using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace FangJia.BusinessLogic.Models.Behaviors;

public class ListBoxBehavior
{
	public static readonly DependencyProperty SelectedItemsSourceProperty =
		DependencyProperty.RegisterAttached(
		                                    "SelectedItemsSource",
		                                    typeof(IList),
		                                    typeof(ListBoxBehavior),
		                                    new FrameworkPropertyMetadata(null,
		                                                                  FrameworkPropertyMetadataOptions
			                                                                  .BindsTwoWayByDefault,
		                                                                  OnSelectedItemsChanged));

	public static void SetSelectedItemsSource(DependencyObject element, IList value)
	{
		element.SetValue(SelectedItemsSourceProperty, value);
	}

	public static IList GetSelectedItemsSource(DependencyObject element)
	{
		return (IList)element.GetValue(SelectedItemsSourceProperty);
	}

	private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not ListView listView) return;
		listView.SelectionChanged -= ListBox_SelectionChanged;
		listView.SelectionChanged += ListBox_SelectionChanged;
	}

	private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is not ListView listView) return;
		var selectedItems = GetSelectedItemsSource(listView);
		selectedItems.Clear();
		foreach (var item in listView.SelectedItems)
		{
			selectedItems.Add(item);
		}
		SetSelectedItemsSource(listView, selectedItems);
	}
}
