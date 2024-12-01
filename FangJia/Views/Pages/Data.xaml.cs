using FangJia.Cores.Interfaces;
using FangJia.Cores.Services;
using FangJia.Cores.Services.NavigationServices;
using FangJia.Models.ConfigModels;
using FangJia.ViewModels.PageViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace FangJia.Views.Pages
{
    /// <summary>
    /// DataPage.xaml 的交互逻辑
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class DataPage
    {
        public DataPage()
        {
            InitializeComponent();
            var pageConfigs = LoadPageConfigurations();

            // 初始化框架导航服务
            var frameNavigationService = new FrameNavigationService(ContentFrame, pageConfigs);

            // 初始化并绑定 ViewModel
            var viewModel = ServiceLocator.GetService<DataViewModel>();
            BindViewModel(viewModel, frameNavigationService);

            // 设置初始视图为 HomePage
            frameNavigationService.NavigateTo("FormulaPage");
        }


        /// <summary>
        /// 从配置文件中加载页面配置。
        /// </summary>
        /// <returns>页面配置的列表。</returns>
        private static List<PageConfig> LoadPageConfigurations()
        {
            var configService = new ConfigurationService(Properties.Resources.MainPagesConfigUri);
            return configService.GetConfig<PageConfig>("DataPages");
        }

        /// <summary>
        /// 将 ViewModel 绑定到当前窗口并设置导航服务。
        /// </summary>
        /// <param name="viewModel">要绑定的 ViewModel。</param>
        /// <param name="navigationService">要添加到 ViewModel 的导航服务。</param>
        private void BindViewModel(DataViewModel viewModel, INavigationService navigationService)
        {
            // 将框架导航服务添加到 ViewModel 中
            viewModel.NavigationService = navigationService;

            // 设置数据上下文以进行绑定
            DataContext = viewModel;
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果用户尝试取消选中，恢复到上一个选中项
            if (sender is ListBox { SelectedItem: null, Items.Count: > 0 } listBox)
            {
                listBox.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : listBox.Items[0];
            }
        }
    }
}
