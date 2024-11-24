using FangJia.Cores.Base;
using FangJia.Cores.Interfaces;

namespace FangJia.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    public Dictionary<string, INavigationService> NavigationServices = [];
    public MainWindowViewModel()
    {
        Logger.Info(message: "初始化主窗口viewmodel");
    }
}