using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Services;
using FangJia.UI.ViewModels.Pages.Data;
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
        DataContext = ServiceLocator.GetService<FormulasViewModel>();
    }
    private void CategoryTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var viewModel = DataContext as FormulasViewModel;
        if (e.NewValue is not Category category) return;
        viewModel!.SelectedCategory = category;
    }
}