using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Services;
using FangJia.UI.ViewModels.Pages.Data;

namespace FangJia.BusinessLogic.Models.Behaviors;

public static partial class RichTextBoxBehavior
{
	public static readonly DependencyProperty TextProperty =
		DependencyProperty.RegisterAttached(
		                                    "Text",
		                                    typeof(string),
		                                    typeof(RichTextBoxBehavior),
		                                    new FrameworkPropertyMetadata(string.Empty,
		                                                                  FrameworkPropertyMetadataOptions
			                                                                  .BindsTwoWayByDefault, OnTextChanged));

	private static bool _isUpdating;

	public static string GetText(DependencyObject obj)               => (string)obj.GetValue(TextProperty);
	public static void   SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not RichTextBox richTextBox || _isUpdating) return;

		_isUpdating = true;

		var newValue = e.NewValue as string;

		// 更新 RichTextBox 内容
		richTextBox.Document=new FlowDocument();

		if (!string.IsNullOrEmpty(newValue))
		{
			// 解析字符串，生成富文本内容
			var flowDocument = ParseToFlowDocument(newValue);
			if (flowDocument != null)
			{
				richTextBox.Document = flowDocument;
			}
		}

		// 添加事件监听器，确保双向绑定
		richTextBox.TextChanged -= RichTextBox_TextChanged;
		richTextBox.TextChanged += RichTextBox_TextChanged;

		_isUpdating = false;
	}

	private static void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is not RichTextBox richTextBox || _isUpdating) return;

		_isUpdating = true;

		// 将 RichTextBox 内容转换为字符串
		var text = ConvertFlowDocumentToString(richTextBox.Document);
		if (GetText(richTextBox) != text)
		{
			SetText(richTextBox, text);
		}

		_isUpdating = false;
	}

	/// <summary>
	/// 将自定义格式字符串解析为 FlowDocument。
	/// </summary>
	private static FlowDocument ParseToFlowDocument(string input)
	{
		var flowDocument = new FlowDocument();
		var paragraph    = new Paragraph();

		// 使用正则表达式解析 [词]{关联药物} 的格式
		var regex     = MyRegex();
		var lastIndex = 0;

		foreach (Match match in regex.Matches(input))
		{
			// 添加普通文本
			if (match.Index > lastIndex)
			{
				paragraph.Inlines.Add(new Run(input.Substring(lastIndex, match.Index - lastIndex)));
			}

			// 提取关键词和药物内容
			var word  = match.Groups["word"].Value;
			var drugs = match.Groups["drugs"].Value.Split(',').Select(drug => drug.Trim()).ToList();

			// 创建显示关键词的 TextBox
			var textBox = new TextBox
			              {
				              Text       = word,
				              IsReadOnly = true,
				              Margin     = new Thickness(2, 0, 2, 0),
				              Style      = Application.Current.FindResource("SongWithDrugStyle") as Style
			              };
			var dataContext = ServiceLocator.GetService<FormulasViewModel>();
			textBox.SetBinding(SourceAttachedBehavior.SourceProperty, new Binding("SelectedFormula.Compositions")
			                                                          {
				                                                          Source = dataContext,
				                                                          Mode   = BindingMode.TwoWay
			                                                          });
			foreach (var filteredList in drugs.Select
				         (drug => (SourceAttachedBehavior.GetSource(textBox) as IList)!
				                  .Cast<FormulationComposition>()
				                  .Where(fc => fc.ToString() == drug)
				                  .ToList()))
			{
				SourceAttachedBehavior.SetSelectedItems
					(textBox, new ObservableCollection<FormulationComposition>(filteredList));
			}

			// 将 TextBox 包装为 InlineUIContainer
			var container = new InlineUIContainer(textBox)
			                {
				                BaselineAlignment = BaselineAlignment.Center
			                };
			paragraph.Inlines.Add(container);

			lastIndex = match.Index + match.Length;
		}

		// 添加剩余普通文本
		if (lastIndex < input.Length)
		{
			paragraph.Inlines.Add(new Run(input.Substring(lastIndex)));
		}

		flowDocument.Blocks.Add(paragraph);
		return flowDocument;
	}

	/// <summary>
	/// 将 FlowDocument 转换为自定义格式字符串。
	/// </summary>
	private static string ConvertFlowDocumentToString(FlowDocument document)
	{
		var result = new StringBuilder();

		foreach (var block in document.Blocks)
		{
			if (block is not Paragraph paragraph) continue;

			foreach (var inline in paragraph.Inlines)
			{
				switch (inline)
				{
					case Run run:
						result.Append(run.Text);
						break;
					case InlineUIContainer { Child: TextBox textBox }:
						var word = textBox.Text;

						// 从绑定的 Source 中提取药物信息
						if (SourceAttachedBehavior.GetSelectedItems(textBox) is IEnumerable<FormulationComposition> associatedDrugs)
						{
							var drugs = string.Join(",", associatedDrugs);
							result.Append($"[{word}]{{{drugs}}}");
						}
						else
						{
							result.Append($"[{word}]");
						}
						break;
				}
			}
		}

		return result.ToString();
	}

	[GeneratedRegex(@"\[(?<word>[^\]]+)\]\{(?<drugs>[^\}]+)\}")]
	private static partial Regex MyRegex();
}
