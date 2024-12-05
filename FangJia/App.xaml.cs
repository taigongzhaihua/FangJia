using FangJia.Cores.Interfaces;
using FangJia.Cores.Services;
using FangJia.Cores.Services.NavigationServices;
using FangJia.DataAccess;
using FangJia.ViewModels;
using FangJia.ViewModels.PageViewModels;
using NLog;
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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    /// <summary>
    /// 静态的命名互斥体，用于确保只有一个应用程序实例在运行。
    /// </summary>
    public static Mutex? Mutex1 { get; private set; }

    private const string MutexName = "FangJia";
    protected override void OnStartup(StartupEventArgs e)
    {

        base.OnStartup(e);

        if (e.Args.Contains("ReStart"))
        {
            PipeService.OnAppRestarted();
        }

        // 检查是否已经有一个实例在运行
        // 创建一个命名互斥体，以确保只有一个应用程序实例在运行。
        Mutex1 = new Mutex(true, MutexName, out var createdNew);

        if (!createdNew)
        {
            // 如果互斥体已经存在，则说明已经有一个实例在运行。
            Logger.Warn("检测到已有实例正在运行，通知已存在的实例");
            PipeService.NotifyExistingInstance(); // 通知已运行的实例激活主窗口。
            Current.Shutdown(); // 关闭当前实例。
            return;
        }

        // 启动管道服务端，用于接收来自其他实例的消息。
        Task.Run(PipeService.StartPipeServer);

        // 初始化服务
        var container = new UnityContainer();
        RegisterServices(container);

        // 初始化皮肤
        ServiceLocator.Initialize(container);
        ServiceLocator.GetService<SkinManagerService>().LoadSkinConfig(
            ServiceLocator.GetService<SettingService>().GetSettingValue("Theme").ToString()!
        );

    }
    private static void RegisterServices(UnityContainer container)
    {
        // 注册长期生命周期服务
        container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
        container.RegisterType<SettingService>(new ContainerControlledLifetimeManager());
        container.RegisterType<SkinManagerService>(new ContainerControlledLifetimeManager());
        container.RegisterType<INavigationService, FrameNavigationService>(
            "MainFrameNavigationService",
            new ContainerControlledLifetimeManager()
            );
        container.RegisterType<INavigationService, FrameNavigationService>(
            "DataContentFrameNavigationService",
            new ContainerControlledLifetimeManager()
            );
        container.RegisterType<ConfigurationService>(
            "PagesConfigService",
            new ContainerControlledLifetimeManager(),
            new InjectionConstructor(FangJia.Properties.Resources.PagesConfigUri)
            );

        // 注册短期生命周期服务
        container.RegisterType<DbManager>(new TransientLifetimeManager());

        // 注册 ViewModel
        container.RegisterType<MainWindowViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<DataViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<SettingViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<HomeViewModel>(new HierarchicalLifetimeManager());
        container.RegisterType<DataFormulasViewModel>(new HierarchicalLifetimeManager());
    }
}