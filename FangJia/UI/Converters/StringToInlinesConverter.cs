using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace FangJia.UI.Converters;

public partial class StringToInlinesConverter : IValueConverter
{
	[GeneratedRegex(@"\[(?<word>.+?)\]\{(?<drug>.+?)\}")]
	private static partial Regex WordDrugRegex();

	[GeneratedRegex(@"(.+?)(?:[，。])")]
	private static partial Regex SentenceRegex();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not string songData) return null;

		var inlines = new ObservableCollection<Inline>();
		var lines   = SentenceRegex().Split(songData);

		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line)) continue;

			var regex     = WordDrugRegex();
			var lastIndex = 0;

			foreach (Match match in regex.Matches(line))
			{
				// 添加普通文字
				if (match.Index > lastIndex)
				{
					var plainText = line.Substring(lastIndex, match.Index - lastIndex);
					AddPlainText(inlines, plainText);
				}

				// 添加标记文字
				var word = match.Groups["word"].Value;
				var drug = match.Groups["drug"].Value;
				AddDrugText(inlines, word, drug);

				lastIndex = match.Index + match.Length;
			}

			// 添加剩余普通文字
			if (lastIndex < line.Length)
			{
				var plainText = line.Substring(lastIndex);
				AddPlainText(inlines, plainText);
			}

			inlines.Add(new LineBreak());
		}

		return inlines;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not ObservableCollection<Inline> inlines) return null;

		var builder = new System.Text.StringBuilder();

		foreach (var inline in inlines)
		{
			switch (inline)
			{
				case Run run:
					builder.Append(run.Text);
					break;

				case InlineUIContainer { Child: TextBlock childTextBlock }:
					var text    = childTextBlock.Text;
					var tooltip = ToolTipService.GetToolTip(childTextBlock) as string;
					if (!string.IsNullOrEmpty(tooltip))
					{
						builder.Append($"[{text}]{{{tooltip}}}");
					}
					else
					{
						builder.Append(text);
					}

					break;

				case LineBreak:
					builder.Append('。');
					break;
			}
		}

		return builder.ToString();
	}

	private static void AddPlainText(ObservableCollection<Inline>? inlines, string text)
	{
		foreach (var container in text.Select(c => CreateInlineContainer(c.ToString())))
		{
			inlines?.Add(container);
		}
	}

	private static void AddDrugText(ObservableCollection<Inline>? inlines, string word, string drug)
	{
		var drugContainer = CreateInlineContainer(word, drug);
		inlines?.Add(drugContainer);
	}

	private static InlineUIContainer CreateInlineContainer(string text, string? toolTip = null)
	{
		if (text.Length == 1)
		{
			// 如果只有一个字，使用较大的字体
			var textBlock = new TextBlock
			                {
				                Text              = text,
				                FontSize          = 36,
				                Foreground        = Application.Current.FindResource("PrimaryTitleBrush") as Brush,
				                VerticalAlignment = VerticalAlignment.Center,
				                Margin            = new Thickness(5, 5, 5, 5), // 设置间距
			                };

			if (string.IsNullOrEmpty(toolTip)) return new InlineUIContainer(textBlock);
			// 高亮显示并添加工具提示
			textBlock.Foreground = Application.Current.FindResource("TertiaryTitleBrush") as Brush;
			var toolTipControl = new ToolTip
			                     {
				                     Margin              = new Thickness(0),
				                     FontSize            = 36,
				                     Padding             = new Thickness(0),
				                     Placement           = PlacementMode.Center,
				                     HorizontalOffset    = 0,
				                     HorizontalAlignment = HorizontalAlignment.Center,
				                     VerticalAlignment   = VerticalAlignment.Center,
				                     Background          = Brushes.White,
				                     IsHitTestVisible    = true,
				                     Content             = CreateInlineContainer(toolTip),
				                     StaysOpen           = true
			                     };
			toolTipControl.PlacementRectangle =
				new
					Rect(Math.Min(0 - toolTip.IndexOf(text, StringComparison.Ordinal) * (toolTipControl.FontSize + 10), 0),
					     0,
					     (toolTipControl.FontSize + 10) * toolTip.Length - 10,
					     (toolTipControl.FontSize + 10));
			textBlock.ToolTip = toolTipControl;
			ToolTipService.SetInitialShowDelay(textBlock, 0);
			return new InlineUIContainer(textBlock);
		}

		var panel = new StackPanel
		            {
			            Orientation       = Orientation.Horizontal,
			            VerticalAlignment = VerticalAlignment.Center
		            };
		var border = new Border
		             {
			             BorderThickness   = new Thickness(0),
			             CornerRadius      = new CornerRadius(0),
			             Padding           = new Thickness(0),
			             Margin            = new Thickness(0),
			             Background        = Brushes.Transparent,
			             Child             = panel,
			             VerticalAlignment = VerticalAlignment.Center
		             };
		foreach (var textBlock in
		         text.Select(c => new TextBlock
		                          {
			                          Text = c.ToString(),
			                          FontSize = 36,
			                          Foreground = Application.Current.FindResource("TertiaryTitleBrush") as Brush,
			                          VerticalAlignment = VerticalAlignment.Center,
			                          Margin = new Thickness(5, 5, 5, 5) // 设置间距
		                          }
		                    )
		        )
		{
			panel.Children.Add(textBlock);
		}

		// 如果有多个字，使用较小的字体
		if (!string.IsNullOrEmpty(toolTip))
		{
			var toolTipControl = new ToolTip
			                     {
				                     Margin              = new Thickness(0),
				                     FontSize            = 36,
				                     Padding             = new Thickness(0),
				                     Placement           = PlacementMode.Center,
				                     HorizontalOffset    = 0,
				                     HorizontalAlignment = HorizontalAlignment.Center,
				                     VerticalAlignment   = VerticalAlignment.Center,
				                     Content             = CreateInlineContainer(toolTip),
				                     Background          = Brushes.White,
				                     StaysOpen           = true,
			                     };
			toolTipControl.PlacementRectangle =
				new
					Rect(Math.Min(0 - toolTip.IndexOf(text, StringComparison.Ordinal) * (toolTipControl.FontSize + 10), 0),
					     0,
					     (toolTipControl.FontSize + 10) * toolTip.Length,
					     toolTipControl.FontSize + 20);
			border.ToolTip = toolTipControl;
		}


		ToolTipService.SetInitialShowDelay(border, 0);
		return new InlineUIContainer(border)
		       {
			       BaselineAlignment = BaselineAlignment.Center
		       };
	}
}
