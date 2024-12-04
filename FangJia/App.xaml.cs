using FangJia.Cores.Interfaces;
using FangJia.Cores.Services;
using FangJia.Cores.Services.NavigationServices;
using FangJia.ViewModels;
using FangJia.ViewModels.PageViewModels;
using System.Windows;
using Unity;
using Unity.Injection;
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
        container.RegisterType<INavigationService, FrameNavigationService>("MainFrameNavigationService", new ContainerControlledLifetimeManager());
        container.RegisterType<INavigationService, FrameNavigationService>("DataContentFrameNavigationService", new ContainerControlledLifetimeManager());
        container.RegisterType<ConfigurationService>("PagesConfigService",
            new InjectionConstructor(FangJia.Properties.Resources.PagesConfigUri));

        container.RegisterType<MainWindowViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<DataViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<SettingViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<HomeViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<DataFormulasViewModel>(new HierarchicalLifetimeManager());


        ServiceLocator.Initialize(container);
        ServiceLocator.GetService<SkinManagerService>().LoadSkinConfig(
            ServiceLocator.GetService<SettingService>().GetSettingValue("Theme").ToString()!
        );

    }
}