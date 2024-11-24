using FangJia.Cores.Interfaces;
using FangJia.Models.ConfigModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace FangJia.Cores.Services.NavigationServices;

/// <summary>
/// FrameNavigationService 类，用于管理 Frame 的导航功能。
/// 提供页面导航、后退和前进的功能。
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class FrameNavigationService : INavigationService
{
    private readonly Frame _frame;
    private readonly Dictionary<string, string> _pageMappings;

    /// <summary>
    /// 构造函数，初始化 FrameNavigationService 实例。
    /// </summary>
    /// <param name="frame">用于导航的 Frame 对象。</param>
    /// <param name="pageConfigs">页面配置的列表，用于映射页面名称到页面 URI。</param>
    public FrameNavigationService(Frame frame, List<PageConfig> pageConfigs)
    {
        // 检查 frame 参数是否为 null
        _frame = frame ?? throw new ArgumentNullException(paramName: nameof(frame));

        // 初始化页面映射字典
        _pageMappings = [];

        // 根据页面配置列表填充页面映射字典
        foreach (var config in pageConfigs)
        {
            _pageMappings[key: config.Name!] = config.Uri!;
        }
    }

    /// <summary>
    /// 导航到指定的视图。
    /// </summary>
    /// <param name="viewName">要导航到的视图名称。</param>
    public void NavigateTo(string viewName)
    {
        // 根据视图名称查找对应的 URI 并导航
        if (_pageMappings.TryGetValue(key: viewName, value: out var uri))
        {
            _frame.Navigate(source: new Uri(uriString: uri, uriKind: UriKind.Relative));
        }
        else
        {
            // 如果找不到视图，抛出异常
            throw new ArgumentException(message: $"View {viewName} not found in configuration.");
        }
    }

    /// <summary>
    /// 导航回到前一个页面。
    /// </summary>
    public void GoBack()
    {
        // 如果可以后退，则执行后退操作
        if (_frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }

    /// <summary>
    /// 导航到下一个页面。
    /// </summary>
    public void GoForward()
    {
        // 如果可以前进，则执行前进操作
        if (_frame.CanGoForward)
        {
            _frame.GoForward();
        }
    }
}
