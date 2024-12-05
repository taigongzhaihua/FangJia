using FangJia.Cores.Interfaces;
using FangJia.Cores.Services;
using FangJia.Cores.Services.NavigationServices;
using FangJia.Models.ConfigModels;
using NLog;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace FangJia.Views;

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
        var viewModel = ServiceLocator.GetService<ViewModels.MainWindow>();

        DataContext = viewModel;
        // 设置初始视图为 HomePage
        frameNavigationService?.NavigateTo("HomePage");
    }

    /// <summary>
    /// 从配置文件中加载页面配置。
    /// </summary>
    /// <returns>页面配置的列表。</returns>
    private static List<PageConfig> LoadPageConfigurations()
    {
        var configService = new ConfigurationService(Properties.Resources.PagesConfigUri);
        return configService.GetConfig<PageConfig>("Pages");
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