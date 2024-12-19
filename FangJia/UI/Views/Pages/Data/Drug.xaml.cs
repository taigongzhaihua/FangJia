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
        if (viewModel.DrugList!.Count > 0) return;
        Loaded += async (_, _) => await viewModel.InitDataTask();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { ContextMenu: not null } button)
            button.ContextMenu.IsOpen = !button.ContextMenu.IsOpen;
    }

    private void Expander_OnCollapsed(object sender, RoutedEventArgs e)
    {
        if (sender is not Expander expander) return;
        if (expander.FindName("ChildListView") is ListView childListView)
            childListView.SelectedIndex = -1;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (sender)
        {
            case ListView { Name: "CategoriesListView" } listView when e.AddedItems.Count == 0:
                listView.SelectedItem = e.RemovedItems[0];
                break;
            case ListView { Name: "ChildListView" } when e.AddedItems.Count == 0:
                return;
            case ListView { Name: "ChildListView" }:
            {
                var viewModel = DataContext as DrugViewModel;
                viewModel!.SelectedDrug = e.AddedItems[0] as FangJia.BusinessLogic.Models.Data.Drug;
                break;
            }
        }
    }
}
