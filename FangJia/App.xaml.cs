using FangJia.Cores.Services;
using FangJia.Views;
using System.Windows;
using Unity;
using Unity.Lifetime;

namespace FangJia;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private void App_Startup(object sender, StartupEventArgs e)
    {
        var container = new UnityContainer();

        // 注册服务和 UI 类
        container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
        container.RegisterType<SettingService>(new ContainerControlledLifetimeManager());
        container.RegisterType<SkinManagerService>(new ContainerControlledLifetimeManager());

        container.RegisterType<MainWindow>();

        ServiceLocator.Initialize(container);
        ServiceLocator.GetService<SkinManagerService>().LoadSkinConfig(
            ServiceLocator.GetService<SettingService>().GetSettingValue("Theme").ToString()!
        );

    }
}