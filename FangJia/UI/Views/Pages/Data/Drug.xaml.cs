using FangJia.BusinessLogic.Services;
using FangJia.UI.ViewModels.Pages.Data;
using System.Windows;
using System.Windows.Controls;

namespace FangJia.UI.Views.Pages.Data;

/// <summary>
/// Medicine.xaml 的交互逻辑
/// </summary>
public partial class Drug
{
    public Drug()
    {
        InitializeComponent();

        var viewModel = ServiceLocator.GetService<DrugViewModel>();
        DataContext = viewModel;
        CrawlerMenu.DataContext = viewModel;
        if (viewModel.ShowingDrugs?.Count > 0) return;
        Loaded += async (_, _) => await viewModel.InitDataTask();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { ContextMenu: not null } button)
            button.ContextMenu.IsOpen = !button.ContextMenu.IsOpen;
    }
}
