using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Services;
using FangJia.BusinessLogic.Services.NavigationServices;
using FangJia.UI.ViewModels;
using NLog;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace FangJia.UI.Views;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public partial class MainWindow
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    // MainWindow 的构造函数，重构了初始化服务和导航的方法
    public MainWindow()
    {
        InitializeComponent();
        InitializeServicesAndNavigation();
    }

    /// <summary>
    /// 初始化配置服务、框架导航，并设置数据上下文。
    /// </summary>
    private void InitializeServicesAndNavigation()
    {


        // 初始化框架导航服务
        var frameNavigationService = ServiceLocator.GetService<INavigationService>("MainFrameNavigationService") as FrameNavigationService;
        frameNavigationService?.SetFrame(MainFrame);

        // 初始化并绑定 ViewModel
        var viewModel = ServiceLocator.GetService<MainWindowViewModel>();

        DataContext = viewModel;
        // 设置初始视图为 HomePage
        viewModel.Init();
        frameNavigationService?.NavigateTo("HomePage");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Logger.Info("用户点击关闭按钮");
        Close();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Logger.Info("用户点击最小化按钮");
        WindowState = WindowState.Minimized;
    }

    private void Maximize_OnClick(object sender, RoutedEventArgs e)
    {
        Logger.Info("用户点击最大化按钮");
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        if (sender is not Button button) return;
        button.Content = WindowState == WindowState.Maximized ? "\ue604" : "\ue600";
        button.ToolTip = WindowState == WindowState.Maximized ? "还原窗口" : "最大化窗口";
    }
}