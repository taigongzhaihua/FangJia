using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models;
using FangJia.BusinessLogic.Models.Config;
using FangJia.BusinessLogic.Services.NavigationServices;
using FangJia.DataAccess;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Unity;

namespace FangJia.UI.ViewModels;

/// <summary>
/// 初始化主窗口 ViewModel。
/// </summary>
/// <remarks>
/// 该方法负责从配置文件中加载主菜单项配置，并初始化主菜单项集合。
/// 同时，它还会设置页面映射并更新页面标题和导航按钮的状态。
/// </remarks>
public partial class MainWindowViewModel(
    [Dependency("MainFrameNavigationService")]
    INavigationService navigationService,
    [Dependency("PagesConfigService")] ConfigurationService pageConfigurationService) : ObservableObject
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // 获取当前类的日志记录器实例

    // 用于存储主菜单项的集合
    [ObservableProperty] private ObservableCollection<MainMenuItem> _menuItems = [];

    // 用于存储当前选中的主菜单项的索引
    [ObservableProperty] private int _menuSelectedIndex;

    // 用于存储当前页面的标题
    [ObservableProperty] private string _pageTitle = null!;

    // 用于存储后退按钮是否可用的状态
    [ObservableProperty] private bool _isBackEnabled;

    // 用于存储前进按钮是否可用的状态
    [ObservableProperty] private bool _isForwardEnabled;

    /// <summary>
    /// 初始化主窗口 ViewModel。
    /// </summary>
    /// <remarks>
    /// 该方法负责从配置文件中加载主菜单项配置，并初始化主菜单项集合。
    /// 同时，它还会设置页面映射并更新页面标题和导航按钮的状态。
    /// 
    /// 具体步骤如下：
    /// 1. 检查导航服务是否为 FrameNavigationService 类型，并设置页面映射。
    /// 2. 从配置文件中加载主菜单项配置。
    /// 3. 遍历配置中的每个菜单项，创建相应的命令并添加到主菜单项集合中。
    /// 4. 设置初始页面标题为第一个菜单项的名称。
    /// 5. 更新导航按钮的状态（如后退和前进按钮的可用性）。
    /// </remarks>
    public void Init()
    {
        // 检查导航服务是否为 FrameNavigationService 类型，并设置页面映射
        if (navigationService is FrameNavigationService frameNavigationService)
        {
            frameNavigationService.SetPageMappings(pageConfigurationService.GetConfig<PageConfig>("Pages"));
        }

        // 记录日志，表示正在初始化主窗口 ViewModel
        Logger.Info(message: "初始化主窗口 view model");

        // 从配置文件中加载主菜单项配置
        var configs = pageConfigurationService.GetConfig<MainMenuItemConfig>("MainMenuItems");

        // 初始化主菜单项集合
        foreach (var item in configs)
        {
            // 创建命令，用于导航到指定的页面
            ICommand command = new RelayCommand(
                () =>
                {
                    // 如果当前页面已经是目标页面，则不执行导航
                    if (item.PageName == navigationService.CurrentViewName()) return;

                    // 导航到目标页面
                    navigationService.NavigateTo(item.PageName);

                    // 更新页面标题为当前菜单项的名称
                    PageTitle = item.Name!;

                    // 更新导航按钮的状态
                    UpdateBackForwardEnable();
                });

            // 将菜单项添加到主菜单项集合中
            MenuItems.Add(new MainMenuItem(item.Name, item.Icon, item.PageName, command));
        }

        // 设置初始页面标题为第一个菜单项的名称
        PageTitle = MenuItems[0].Name!;
    }

    /// <summary>
    /// 处理框架后退操作的命令。
    /// </summary>
    /// <remarks>
    /// 该方法执行以下步骤：
    /// 1. 调用导航服务的后退方法。
    /// 2. 更新主菜单选中项的索引。
    /// 3. 更新页面标题为当前选中菜单项的名称。
    /// 4. 更新导航按钮的状态（如后退和前进按钮的可用性）。
    /// </remarks>
    [RelayCommand]
    private void FrameBack()
    {
        // 调用导航服务的后退方法
        navigationService.GoBack();

        // 更新主菜单选中项的索引
        UpdateMenuSelectedIndex();

        // 更新页面标题为当前选中菜单项的名称
        PageTitle = MenuItems[MenuSelectedIndex].Name!;

        // 更新导航按钮的状态
        UpdateBackForwardEnable();
    }

    /// <summary>
    /// 处理框架前进操作的命令。
    /// </summary>
    /// <remarks>
    /// 该方法执行以下步骤：
    /// 1. 调用导航服务的前进方法。
    /// 2. 更新主菜单选中项的索引。
    /// 3. 更新页面标题为当前选中菜单项的名称。
    /// 4. 更新导航按钮的状态（如后退和前进按钮的可用性）。
    /// </remarks>
    [RelayCommand]
    private void FrameForward()
    {
        // 调用导航服务的前进方法
        navigationService.GoForward();

        // 更新主菜单选中项的索引
        UpdateMenuSelectedIndex();

        // 更新页面标题为当前选中菜单项的名称
        PageTitle = MenuItems[MenuSelectedIndex].Name!;

        // 更新导航按钮的状态
        UpdateBackForwardEnable();
    }

    /// <summary>
    /// 更新主菜单选中项的索引。
    /// </summary>
    /// <remarks>
    /// 该方法执行以下步骤：
    /// 1. 获取当前导航服务中显示的页面名称。
    /// 2. 在主菜单项集合中查找与当前页面名称匹配的菜单项。
    /// 3. 获取匹配菜单项的索引，并更新主菜单选中项的索引。
    /// </remarks>
    private void UpdateMenuSelectedIndex()
    {
        // 获取当前导航服务中显示的页面名称，并在主菜单项集合中查找匹配的菜单项
        MenuSelectedIndex =
            MenuItems.IndexOf(
                MenuItems.FirstOrDefault(x => navigationService.CurrentViewName()!.Contains(x.PageName!))!);
    }

    /// <summary>
    /// 更新导航按钮的状态（如后退和前进按钮的可用性）。
    /// </summary>
    /// <remarks>
    /// 该方法执行以下步骤：
    /// 1. 检查导航服务是否可以后退，并更新后退按钮的可用性。
    /// 2. 检查导航服务是否可以前进，并更新前进按钮的可用性。
    /// </remarks>
    private void UpdateBackForwardEnable()
    {
        // 检查导航服务是否可以后退，并更新后退按钮的可用性
        IsBackEnabled = navigationService.CanGoBack;

        // 检查导航服务是否可以前进，并更新前进按钮的可用性
        IsForwardEnabled = navigationService.CanGoForward;
    }
}