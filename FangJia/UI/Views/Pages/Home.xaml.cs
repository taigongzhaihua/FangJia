using System.Windows;
using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Services;
using FangJia.BusinessLogic.Services.NavigationServices;

namespace FangJia.UI.Views.Pages;

/// <summary>
/// Home.xaml 的交互逻辑
/// </summary>
public partial class Home
{
	/// <summary>
	/// 构造函数，初始化 Home 页面
	/// </summary>
	public Home()
	{
		// 调用 InitializeComponent 方法以初始化页面组件
		InitializeComponent();
	}

	private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
	{
		var frameNavigationService =
			ServiceLocator.GetService<INavigationService>
				("MainFrameNavigationService") as FrameNavigationService;
		frameNavigationService?.NavigateTo("HomePage-MemorizationPage");
	}
}
