using FangJia.Cores.Base;
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
    public Dictionary<string, INavigationService> NavigationServices = [];
    public ObservableCollection<MainMenuItemData> MenuItems { get; set; } = [];
    public MainWindowViewModel()
    {
        Logger.Info(message: "初始化主窗口viewmodel");

        var configService = new ConfigurationService(configFilePath: Properties.Resources.MainPagesConfigUri);
        var configs = configService.GetConfig<MainMenuItemConfig>("MainMenuItems");

        foreach (var item in configs!)
        {
            ICommand command = new RelayCommand(o => NavigationServices["MainFrame"].NavigateTo(item.PageName));
            MenuItems!.Add(new MainMenuItemData(item.Name, item.Icon, command));
        }

    }
}