using FangJia.Models.DataModel;
using FangJia.ViewModels.PageViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FangJia.Views.Pages.Data;

/// <summary>
/// Fomulas.xaml 的交互逻辑
/// </summary>
public partial class Formulas : Page
{
    public Formulas()
    {
        InitializeComponent();
    }
    private void CategoryTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var viewModel = DataContext as DataFormulasViewModel;
        if (e.NewValue is not Category category) return;
        viewModel!.SelectedCategory = category;
    }
}