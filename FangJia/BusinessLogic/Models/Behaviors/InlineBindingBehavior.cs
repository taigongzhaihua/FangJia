using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FangJia.BusinessLogic.Models.Behaviors;

public class InlineBindingBehavior
{
	public static readonly DependencyProperty InlineSourceProperty =
		DependencyProperty.RegisterAttached(
		                                    "InlineSource",
		                                    typeof(ObservableCollection<Inline>),
		                                    typeof(InlineBindingBehavior),
		                                    new PropertyMetadata(null, OnInlineSourceChanged));

	public static void SetInlineSource(DependencyObject element, ObservableCollection<Inline> value)
	{
		element.SetValue(InlineSourceProperty, value);
	}

	public static ObservableCollection<Inline> GetInlineSource(DependencyObject element)
	{
		return (ObservableCollection<Inline>)element.GetValue(InlineSourceProperty);
	}

	private static void OnInlineSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TextBlock textBlock)
		{
			textBlock.Inlines.Clear();
			if (e.NewValue is ObservableCollection<Inline> inlines)
			{
				foreach (var inline in inlines)
				{
					textBlock.Inlines.Add(inline);
				}
			}
		}
	}
}
