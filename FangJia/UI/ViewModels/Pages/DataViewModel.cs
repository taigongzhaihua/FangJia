using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Config;
using FangJia.BusinessLogic.Models.Utils;
using FangJia.BusinessLogic.Services.NavigationServices;
using FangJia.DataAccess;
using FangJia.UI.Base;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Unity;
using TabItem = FangJia.BusinessLogic.Models.Data.TabItem;

namespace FangJia.UI.ViewModels.Pages;

public class Data : ViewModelBase
{

    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly INavigationService _navigationService;

    // 用于存储主菜单项的集合
    public ObservableCollection<TabItem> TabItems { get; set; } = [];

    private int _tabSelectedIndex;
    public int TabSelectedIndex
    {
        get => _tabSelectedIndex;
        set => SetProperty(ref _tabSelectedIndex, value);
    }
    public Data(
        [Dependency("DataContentFrameNavigationService")] INavigationService navigationService,
        [Dependency("PagesConfigService")] ConfigurationService pageConfigurationService
        )
    {
        _navigationService = navigationService;
        if (_navigationService is FrameNavigationService frameNavigationService)
        {
            frameNavigationService.SetPageMappings(pageConfigurationService.GetConfig<PageConfig>("DataPages"));
        }
        Logger.Info(message: "初始化主窗口viewmodel");

        // 从配置文件中加载主菜单项配置

        var configs = pageConfigurationService.GetConfig<TabItem>("DataTabs");

        // 初始化主菜单项集合
        foreach (var item in configs)
        {
            ICommand command = new RelayCommand(
                _ =>
                {
                    if (item.PageName == _navigationService.CurrentViewName()) return;
                    _navigationService.NavigateTo(item.PageName);
                });
            TabItems.Add(new TabItem(item.Name, item.PageName, command));
        }
    }
    public void UpdateTabSelectedIndex()
    {
        TabSelectedIndex = TabItems.IndexOf(TabItems.FirstOrDefault(x => _navigationService.CurrentViewName()!.Contains(x.PageName!))!);
    }

}