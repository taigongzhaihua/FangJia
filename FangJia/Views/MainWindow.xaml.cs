using FangJia.Cores.Interfaces;
using FangJia.Cores.Services;
using FangJia.Cores.Services.NavigationServices;
using FangJia.Models.ConfigModels;
using FangJia.ViewModels;
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
        // 加载页面配置
        var pageConfigs = LoadPageConfigurations();

        // 初始化框架导航服务
        var frameNavigationService = new FrameNavigationService(MainFrame, pageConfigs);

        // 初始化并绑定 ViewModel
        var viewModel = ServiceLocator.GetService<MainWindowViewModel>();
        BindViewModel(viewModel, frameNavigationService);

        // 设置初始视图为 HomePage
        frameNavigationService.NavigateTo("HomePage");
    }

    /// <summary>
    /// 从配置文件中加载页面配置。
    /// </summary>
    /// <returns>页面配置的列表。</returns>
    private static List<PageConfig> LoadPageConfigurations()
    {
        var configService = new ConfigurationService(Properties.Resources.MainPagesConfigUri);
        return configService.GetConfig<PageConfig>("Pages");
    }

    /// <summary>
    /// 将 ViewModel 绑定到当前窗口并设置导航服务。
    /// </summary>
    /// <param name="viewModel">要绑定的 ViewModel。</param>
    /// <param name="navigationService">要添加到 ViewModel 的导航服务。</param>
    private void BindViewModel(MainWindowViewModel viewModel, INavigationService navigationService)
    {
        // 将框架导航服务添加到 ViewModel 中
        viewModel.NavigationServices.Add("MainFrame", navigationService);

        // 设置数据上下文以进行绑定
        DataContext = viewModel;
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