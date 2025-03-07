﻿using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Services;
using FangJia.BusinessLogic.Services.NavigationServices;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace FangJia.UI.Views.Pages;

/// <summary>
/// DataPage.xaml 的交互逻辑
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public partial class DataPage
{
	public DataPage()
	{
		InitializeComponent();

		// 初始化框架导航服务
		var frameNavigationService =
			ServiceLocator.GetService<INavigationService>
					("DataContentFrameNavigationService")
				as FrameNavigationService;
		frameNavigationService?.SetFrame(ContentFrame);

		// 初始化并绑定 ViewModel
		var viewModel = ServiceLocator.GetService<ViewModels.Pages.DataViewModel>();
		DataContext = viewModel;
		viewModel.InitPageData();
		// 设置初始视图为 FormulaPage
		frameNavigationService?.NavigateTo("FormulaPage");
		viewModel.UpdateTabSelectedIndex();
	}

	private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		// 如果用户尝试取消选中，恢复到上一个选中项
		if (sender is ListBox { SelectedItem: null, Items.Count: > 0 } listBox)
			listBox.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : listBox.Items[0];
	}
}
