using FangJia.Cores.Services;
using FangJia.Cores.Services.NavigationServices;
using FangJia.Models;
using FangJia.ViewModels;

namespace FangJia.Views;
public partial class MainWindow
{

    public MainWindow()
    {
        InitializeComponent();
        var configService = new ConfigurationService("Configs/pagesConfig.json");
        var pageConfigs = configService.GetConfig<PageConfig>("Pages");
        var frameNavigationService = new FrameNavigationService(MainFrame, pageConfigs);
        var viewModel = new MainWindowViewModel();
        viewModel.NavigationServices.Add("Frame", frameNavigationService);
        DataContext = viewModel;
        frameNavigationService.NavigateTo("HomePage");
    }

}