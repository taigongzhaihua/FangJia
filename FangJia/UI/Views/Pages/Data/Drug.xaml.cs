using FangJia.BusinessLogic.Services;
using FangJia.UI.ViewModels.Pages.Data;

namespace FangJia.UI.Views.Pages.Data;

/// <summary>
/// Medicine.xaml 的交互逻辑
/// </summary>
public partial class Drug
{
    public Drug()
    {
        InitializeComponent();
        DataContext = ServiceLocator.GetService<DrugViewModel>();
    }
}