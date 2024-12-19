using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Services;
using FangJia.UI.ViewModels.Pages.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
		DataContext             = viewModel;
		CrawlerMenu.DataContext = viewModel;
		if (viewModel.Categories.Count > 0) return;
		Loaded += async (_, _) => { await viewModel.InitDataTask(); };
	}

	private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
	{
		if (sender is Button { ContextMenu: not null } button)
			button.ContextMenu.IsOpen = !button.ContextMenu.IsOpen;
	}


	private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var viewModel = DataContext as FormulasViewModel;
		if (e.AddedItems.Count == 0)
		{
			if (sender is ListView { Name: "CategoriesListView" } listView)
			{
				listView.SelectedItem = e.RemovedItems[0];
				var listViewItem =
					listView.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) as ListViewItem;
				// 获取 ListViewItem 的 ContentPresenter
				var contentPresenter = FindChild<ContentPresenter>(listViewItem!);
				// 查找 Expander 的模板
				if (contentPresenter?.ContentTemplate
				                    ?.FindName("ChildListView", contentPresenter)
				    is ListView childListView)
					childListView.SelectedIndex = -1;
			}

			return;
		}

		if (e.AddedItems[0] is not Category category) return;
		viewModel!.SelectedCategory = category;
	}

	private void Expander_OnCollapsed(object sender, RoutedEventArgs e)
	{
		if (sender is not Expander expander) return;
		if (expander.FindName("ChildListView") is ListView childListView)
			childListView.SelectedIndex = -1;
	}

// 通用递归查找方法
	private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
	{
		for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			var child = VisualTreeHelper.GetChild(parent, i);

			if (child is T typedChild)
			{
				return typedChild;
			}

			var result = FindChild<T>(child);
			if (result != null)
			{
				return result;
			}
		}

		return null;
	}
}
