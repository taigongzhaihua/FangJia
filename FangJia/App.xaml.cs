using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Data;
using FangJia.BusinessLogic.Services;
using FangJia.BusinessLogic.Services.Crawlers;
using FangJia.BusinessLogic.Services.NavigationServices;
using FangJia.DataAccess;
using FangJia.UI.ViewModels;
using FangJia.UI.ViewModels.Pages;
using FangJia.UI.ViewModels.Pages.Data;
using NLog;
using System.Windows;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace FangJia;

/// <summary>
/// 应用程序的主入口点。
/// 该类继承自Application类，负责应用程序的启动、初始化和服务的注册。
/// </summary>
public partial class App
{
	private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// 互斥体的名称，用于标识应用程序的唯一实例。
	/// </summary>
	private const string MutexName = "FangJia";

	/// <summary>
	/// 应用程序启动时调用的方法。
	/// </summary>
	/// <param name="e">启动事件参数，包含命令行参数。</param>
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		// 步骤1：检查是否是重启
		if (e.Args.Contains("ReStart"))
		{
			PipeService.OnAppRestarted();
		}

		// 步骤2：检查是否已经有一个实例在运行
		// 创建一个命名互斥体，以确保只有一个应用程序实例在运行。
		new Mutex(true, MutexName, out var createdNew);
		if (!createdNew)
		{
			// 如果互斥体已经存在，则说明已经有一个实例在运行。
			Logger.Warn("检测到已有实例正在运行，通知已存在的实例");
			PipeService.NotifyExistingInstance(); // 通知已运行的实例激活主窗口。
			Current.Shutdown();                   // 关闭当前实例。
			return;
		}

		// 步骤3：启动管道服务端，用于接收来自其他实例的消息。
		Task.Run(PipeService.StartPipeServer); // 启动管道服务端

		// 步骤4：初始化服务
		var container = new UnityContainer(); // 创建一个Unity容器
		RegisterServices(container);          // 注册服务

		// 步骤5：初始化皮肤
		ServiceLocator.Initialize(container);
		ServiceLocator.GetService<SkinManagerService>()
		              .LoadSkinConfig(SettingService.GetSettingValue("Theme")
		                                            .ToString()!);
	}

	/// <summary>
	/// 注册服务的方法。
	/// </summary>
	/// <param name="container">Unity容器，用于注册服务。</param>
	private static void RegisterServices(UnityContainer container)
	{
		// 步骤1：注册长期生命周期服务
		container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager()); // 事件聚合器
		container.RegisterType<SettingService>(new ContainerControlledLifetimeManager());                    // 设置服务
		container.RegisterType<SkinManagerService>(new ContainerControlledLifetimeManager());                // 皮肤管理服务
		container.RegisterType<INavigationService, FrameNavigationService>                                   // 主窗口导航服务
			("MainFrameNavigationService",
			 new ContainerControlledLifetimeManager());
		container.RegisterType<INavigationService, FrameNavigationService> // 数据管理页面导航服务
			("DataContentFrameNavigationService",
			 new ContainerControlledLifetimeManager());
		container.RegisterType<ConfigurationService> // 页面配置服务
			("PagesConfigService",
			 new ContainerControlledLifetimeManager(),
			 new InjectionConstructor(FangJia.Properties.Resources
			                                 .PagesConfigUri));
		container.RegisterType<DataService>(new ContainerControlledLifetimeManager());
		container.RegisterType<DbManager>(new ContainerControlledLifetimeManager()); // 数据库管理类
		container.RegisterType<ICrawler<Drug>, DrugCrawler>                          // 药物爬虫
			("DrugCrawler",
			 new ContainerControlledLifetimeManager());
		container.RegisterType<ICrawler<Formulation>, FormulationCrawler> // 方剂爬虫
			("FormulationCrawler",
			 new ContainerControlledLifetimeManager());
		container.RegisterType<ICrawler<(string Category, string FormulaName)>, FangjiCrawler>
			("FangjiCrawler",
			 new ContainerControlledLifetimeManager()); // 方剂、分类对照表爬虫

		// 步骤2：注册 ViewModel
		container.RegisterType<MainWindowViewModel>(new HierarchicalLifetimeManager()); // 主窗口 ViewModel
		container.RegisterType<DataViewModel>(new HierarchicalLifetimeManager());       // 数据管理页面 ViewModel
		container.RegisterType<SettingViewModel>(new HierarchicalLifetimeManager());    // 设置页面 ViewModel
		container.RegisterType<HomeViewModel>(new HierarchicalLifetimeManager());       // 主页 ViewModel
		container.RegisterType<FormulasViewModel>(new HierarchicalLifetimeManager());   // 数据管理页面-方剂页 ViewModel
		container.RegisterType<DrugViewModel>(new HierarchicalLifetimeManager());       // 数据管理页面-药物页 ViewModel
	}
}
