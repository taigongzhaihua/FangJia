using FangJia.BusinessLogic.Services;
using FangJia.UI.ViewModels.Pages;

namespace FangJia.UI.Views.Pages;

public partial class Memorization
{
	public Memorization()
	{
		InitializeComponent();
		var viewModel = ServiceLocator.GetService<MemorizationViewModel>();
		DataContext = viewModel;

	}
}
