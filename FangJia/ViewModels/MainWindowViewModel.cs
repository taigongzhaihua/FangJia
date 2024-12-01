﻿using FangJia.Cores.Base;
using FangJia.Cores.Interfaces;
using FangJia.Cores.Services;
using FangJia.Cores.Utils.Commands;
using FangJia.Models;
using FangJia.Models.ConfigModels;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace FangJia.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // 用于存储导航服务的字典
    public Dictionary<string, INavigationService> NavigationServices = [];

    // 用于存储主菜单项的集合
    public ObservableCollection<MainMenuItemData> MenuItems { get; set; } = [];

    // 用于存储当前选中的主菜单项的索引
    private int _menuSelectedIndex;
    public int MenuSelectedIndex
    {
        get => _menuSelectedIndex;
        set => SetProperty(ref _menuSelectedIndex, value);
    }

    private string _pageTitle = null!;
    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    // 用于存储返回和前进命令
    public ICommand? FrameBackCommand { get; set; }
    public ICommand? FrameForwardCommand { get; set; }

    /// <summary>
    /// 初始化主窗口 ViewModel。
    /// </summary>
    /// <remarks>
    /// 从配置文件中加载主菜单项配置，初始化主菜单项集合。
    /// </remarks>
    public MainWindowViewModel()
    {
        Logger.Info(message: "初始化主窗口viewmodel");

        // 从配置文件中加载主菜单项配置
        var configService = new ConfigurationService(Properties.Resources.MainPagesConfigUri);
        var configs = configService.GetConfig<MainMenuItemConfig>("MainMenuItems");

        // 初始化主菜单项集合
        foreach (var item in configs)
        {
            ICommand command = new RelayCommand(
                _ =>
                {
                    if (item.PageName == NavigationServices["MainFrame"].CurrentViewName()) return;
                    NavigationServices["MainFrame"].NavigateTo(item.PageName);
                    PageTitle = item.Name!;
                });
            MenuItems.Add(new MainMenuItemData(item.Name, item.Icon, item.PageName, command));
        }

        // 初始化命令
        InitCommands();
        PageTitle = MenuItems[0].Name!;
    }

    /// <summary>
    /// 初始化命令。
    /// </summary>
    private void InitCommands()
    {
        FrameBackCommand = new RelayCommand(
            _ =>
            {
                NavigationServices["MainFrame"].GoBack();
                UpdateMenuSelectedIndex();
                PageTitle = MenuItems[MenuSelectedIndex].Name!;
            });
        FrameForwardCommand = new RelayCommand(
            _ =>
            {
                NavigationServices["MainFrame"].GoForward();
                UpdateMenuSelectedIndex();
                PageTitle = MenuItems[MenuSelectedIndex].Name!;
            });
    }

    /// <summary>
    /// 更新主菜单选中项的索引。
    /// </summary>
    private void UpdateMenuSelectedIndex()
    {
        MenuSelectedIndex = MenuItems.IndexOf(MenuItems.FirstOrDefault(x => NavigationServices["MainFrame"].CurrentViewName()!.Contains(x.PageName!))!);
    }


}