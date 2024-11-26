using FangJia.Cores.Services;
using System.Windows;

namespace FangJia
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // 启动时加载皮肤管理服务
            SkinManagerService.Instance.LoadSkinConfig("lightTheme");  // 你可以指定默认的主题名称
        }
    }

}
