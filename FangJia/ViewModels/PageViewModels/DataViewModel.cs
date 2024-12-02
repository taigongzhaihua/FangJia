using FangJia.Cores.Base;
using FangJia.Cores.Interfaces;
using FangJia.Cores.Services;
using FangJia.Cores.Utils.Commands;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TabItem = FangJia.Models.DataModel.TabItem;

namespace FangJia.ViewModels.PageViewModels;

public class DataViewModel : BaseViewModel
{

    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public INavigationService NavigationService { get; set; }

    // 用于存储主菜单项的集合
    public ObservableCollection<TabItem> TabItems { get; set; } = [];

    private int _tabSelectedIndex;
    public int TabSelectedIndex
    {
        get => _tabSelectedIndex;
        set => SetProperty(ref _tabSelectedIndex, value);
    }
    public DataViewModel()
    {
        Logger.Info(message: "初始化主窗口viewmodel");

        // 从配置文件中加载主菜单项配置
        var configService = new ConfigurationService(Properties.Resources.MainPagesConfigUri);
        var configs = configService.GetConfig<TabItem>("DataTabs");

        // 初始化主菜单项集合
        foreach (var item in configs)
        {
            ICommand command = new RelayCommand(
                _ =>
                {
                    if (item.PageName == NavigationService!.CurrentViewName()) return;
                    NavigationService.NavigateTo(item.PageName);
                });
            TabItems.Add(new TabItem(item.Name, item.PageName, command));
        }
    }
    public void UpdateTabSelectedIndex()
    {
        TabSelectedIndex = TabItems.IndexOf(TabItems.FirstOrDefault(x => NavigationService.CurrentViewName()!.Contains(x.PageName!))!);
    }

}