using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Config;
using FangJia.BusinessLogic.Services.NavigationServices;
using FangJia.DataAccess;
using NLog;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Unity;
using TabItem = FangJia.BusinessLogic.Models.Data.TabItem;

namespace FangJia.UI.ViewModels.Pages;

/// <summary>
/// 抽象部分类 DataViewModel，继承自 ObservableObject。
/// 该类通过构造函数注入依赖项，包括导航服务和页面配置服务。
/// </summary>
/// <param name="navigationService">
/// 导航服务，用于页面导航。通过 Unity 容器注入，依赖名称设置为 "DataContentFrameNavigationService"。
/// </param>
/// <param name="pageConfigurationService">
/// 页面配置服务，用于获取页面配置信息。通过 Unity 容器注入，依赖名称设置为 "PagesConfigService"。
/// </param>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class DataViewModel(
    [Dependency("DataContentFrameNavigationService")]
    INavigationService navigationService,
    [Dependency("PagesConfigService")] ConfigurationService pageConfigurationService)
    : ObservableObject
{
    /// <summary>
    /// 用于记录日志的静态Logger实例，通过NLog库获取当前类的Logger。
    /// </summary>
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 用于存储主菜单项的集合，使用ObservableCollection以便在集合变化时通知UI更新。
    /// </summary>
    [ObservableProperty] private ObservableCollection<TabItem> _tabItems = [];

    /// <summary>
    /// 用于存储当前选中的主菜单项索引，使用ObservableProperty以便在值变化时通知UI更新。
    /// </summary>
    [ObservableProperty] private int _tabSelectedIndex;

    /// <summary>
    /// 初始化页面数据的方法。
    /// 该方法执行以下步骤：
    /// 1. 检查导航服务是否为FrameNavigationService类型，并设置页面映射。
    /// 2. 记录日志，表示正在初始化主窗口ViewModel。
    /// 3. 从配置文件中加载主菜单项配置。
    /// 4. 清空现有的主菜单项集合。
    /// 5. 初始化主菜单项集合，并为每个菜单项创建导航命令。
    /// </summary>
    public void InitPageData()
    {
        // 步骤1: 检查导航服务是否为FrameNavigationService类型，并设置页面映射
        if (navigationService is FrameNavigationService frameNavigationService)
        {
            frameNavigationService.SetPageMappings(pageConfigurationService.GetConfig<PageConfig>("DataPages"));
        }

        // 步骤2: 记录日志，表示正在初始化主窗口ViewModel
        Logger.Info(message: "初始化主窗口 view model");

        // 步骤3: 从配置文件中加载主菜单项配置
        var configs = pageConfigurationService.GetConfig<TabItem>("DataTabs");

        // 步骤4: 清空现有的主菜单项集合
        TabItems.Clear();

        // 步骤5: 初始化主菜单项集合
        foreach (var item in configs)
        {
            // 创建一个命令，用于导航到指定的页面
            ICommand command = new RelayCommand(
                () =>
                {
                    // 如果当前页面已经是目标页面，则不执行导航
                    if (item.PageName == navigationService.CurrentViewName()) return;
                    // 导航到目标页面
                    navigationService.NavigateTo(item.PageName);
                });

            // 将新的菜单项添加到主菜单项集合中
            TabItems.Add(new TabItem(item.Name, item.PageName, command));
        }
    }


    /// <summary>
    /// 更新当前选中的主菜单项索引的方法。
    /// </summary>
    public void UpdateTabSelectedIndex()
    {
        // 步骤1: 查找当前显示的页面名称在主菜单项集合中的索引
        TabSelectedIndex =
            TabItems.IndexOf(TabItems.FirstOrDefault(x => navigationService.CurrentViewName()!.Contains(x.PageName!))!);
    }
}
