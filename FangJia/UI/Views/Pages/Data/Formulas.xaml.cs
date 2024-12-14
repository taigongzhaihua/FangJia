using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Services;
using FangJia.UI.ViewModels.Pages.Data;
using System.Windows;
using System.Windows.Controls;

namespace FangJia.UI.Views.Pages.Data;

/// <summary>
/// Formulas.xaml 的交互逻辑
/// </summary>
public partial class Formulas
{
	public Formulas()
	{
		InitializeComponent();
		var viewModel = ServiceLocator.GetService<FormulasViewModel>();
		DataContext = viewModel;
		if (viewModel.Categories.Count > 0) return;
		Loaded += async (_, _) => { await viewModel.InitDataTask(); };
	}

	private void CategoryTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		var viewModel = DataContext as FormulasViewModel;
		if (e.NewValue is not Category category) return;
		viewModel!.SelectedCategory = category;
	}

	private bool _isInitialized;

	private void TextBox_Loaded(object sender, RoutedEventArgs e)
	{
		// 标志控件已初始化完成
		_isInitialized = true;
	}

	private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
	{
		if (!_isInitialized) return;

		if (sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
			SaveButton.IsEnabled = true;
		else
			SaveButton.IsEnabled = false;
	}
}
