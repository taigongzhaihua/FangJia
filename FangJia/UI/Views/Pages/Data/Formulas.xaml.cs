using FangJia.BusinessLogic.Models.Data;
using FangJia.UI.ViewModels.PageViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FangJia.UI.Views.Pages.Data;

/// <summary>
/// Formulas.xaml 的交互逻辑
/// </summary>
public partial class Formulas : Page
{
    public Formulas()
    {
        InitializeComponent();
    }
    private void CategoryTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var viewModel = DataContext as DataFormulas;
        if (e.NewValue is not Category category) return;
        viewModel!.SelectedCategory = category;
    }
}