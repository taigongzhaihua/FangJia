using System.Diagnostics.CodeAnalysis;
using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Config;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FangJia.BusinessLogic.Services.NavigationServices;

/// <summary>
/// 提供页面导航服务的类，实现了 INavigationService 接口。
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class FrameNavigationService : INavigationService
{
    private Frame _frame = null!;
    private readonly Dictionary<string, string> _pageMappings = [];
    private string? _currentViewName;
    private readonly Stack<string?> _backNameStack = new();
    private readonly Stack<string?> _forwardNameStack = new();

    /// <summary>
    /// 判断是否可以返回上一个页面。
    /// </summary>
    public bool CanGoBack => _backNameStack.Count > 0;

    /// <summary>
    /// 判断是否可以前进到下一个页面。
    /// </summary>
    public bool CanGoForward => _forwardNameStack.Count > 0;

    /// <summary>
    /// 设置导航框架。
    /// </summary>
    /// <param name="frame">要设置的导航框架。</param>
    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    /// <summary>
    /// 设置页面映射配置。
    /// </summary>
    /// <param name="pageConfigs">页面配置列表，包含页面名称和对应的URI。</param>
    public void SetPageMappings(List<PageConfig> pageConfigs)
    {
        // 清空现有的页面映射
        _pageMappings.Clear();

        // 遍历页面配置列表，将每个页面的名称和URI添加到映射字典中
        foreach (var config in pageConfigs)
        {
            _pageMappings[config.Name!] = config.Uri!;
        }
    }

    /// <summary>
    /// 获取当前视图的名称。
    /// </summary>
    /// <returns>当前视图的名称，如果未设置则返回null。</returns>
    public string? CurrentViewName()
    {
        return _currentViewName;
    }

    /// <summary>
    /// 导航到指定视图页面。
    /// </summary>
    /// <param name="viewName">目标视图的名称。</param>
    /// <remarks>
    /// 实现步骤：
    /// 1. 检查目标视图名称是否与当前视图名称相同，如果相同且当前页面内容不为空，则直接返回。
    /// 2. 根据视图名称从页面映射字典中获取对应的URI。
    /// 3. 获取当前页面的内容，用于执行页面切换动画。
    /// 4. 调用 <see cref="StartPageTransitionAnimation"/> 方法执行页面切换动画。
    /// 5. 更新视图信息，将当前视图名称压入后退栈，并清空前进栈。
    /// 6. 更新当前视图名称为目标视图名称。
    /// 7. 如果目标视图名称在页面映射字典中不存在，则抛出 <see cref="ArgumentException"/> 异常。
    /// </remarks>
    public void NavigateTo(string? viewName)
    {
        // 检查目标视图名称是否与当前视图名称相同，如果相同且当前页面内容不为空，则直接返回
        if (viewName == CurrentViewName() && _frame.Content != null) return;

        // 根据视图名称从页面映射字典中获取对应的URI
        if (_pageMappings.TryGetValue(viewName!, out var uri))
        {
            // 获取当前页面的内容，用于执行页面切换动画
            var currentContent = _frame.Content as UIElement;

            // 调用 StartPageTransitionAnimation 方法执行页面切换动画
            StartPageTransitionAnimation(currentContent, uri);

            // 更新视图信息
            if (!string.IsNullOrEmpty(_currentViewName))
            {
                // 将当前视图名称压入后退栈
                _backNameStack.Push(CurrentViewName());
                // 清空前进栈
                _forwardNameStack.Clear();
            }

            // 更新当前视图名称为目标视图名称
            _currentViewName = viewName!;
        }
        else
        {
            // 如果目标视图名称在页面映射字典中不存在，则抛出异常
            throw new ArgumentException($"View {viewName} not found in configuration.");
        }
    }

    /// <summary>
    /// 执行页面切换动画。
    /// </summary>
    /// <param name="currentContent">当前页面的内容。</param>
    /// <param name="uri">目标页面的URI。</param>
    /// <remarks>
    /// 实现步骤：
    /// 1. 检查当前页面内容是否为空。
    /// 2. 如果当前页面内容为空，直接导航到目标页面，并应用淡入动画。
    /// 3. 如果当前页面内容不为空，执行当前页面的淡出动画，动画结束后导航到目标页面，并应用淡入动画。
    /// </remarks>
    private void StartPageTransitionAnimation(UIElement? currentContent, string uri)
    {
        if (currentContent == null)
        {
            // 如果当前页面内容为空，直接导航到目标页面
            _frame.Navigate(new Uri(uri, UriKind.Relative));

            // 应用淡入动画
            ApplyFadeInAnimation(_frame);
        }
        else
        {
            // 如果当前页面内容不为空，执行当前页面的淡出动画
            ApplyFadeOutAnimation(uri);
        }
    }

    /// <summary>
    /// 执行当前页面的淡出动画。
    /// </summary>
    /// <param name="uri">目标页面的URI。</param>
    /// <remarks>
    /// 实现步骤：
    /// 1. 创建淡出动画，设置动画的起始值为1，结束值为0，持续时间为0.2秒。
    /// 2. 在动画完成事件中，导航到目标页面，并应用淡入动画。
    /// 3. 如果导航框架支持后退操作，移除后退历史记录。
    /// 4. 启动淡出动画。
    /// </remarks>
    private void ApplyFadeOutAnimation(string uri)
    {
        // 创建淡出动画，设置动画的起始值为1，结束值为0，持续时间为0.2秒
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        // 在动画完成事件中，导航到目标页面，并应用淡入动画
        fadeOutAnimation.Completed += (_, _) =>
        {
            _frame.Navigate(new Uri(uri, UriKind.Relative));

            // 应用淡入动画
            ApplyFadeInAnimation(_frame);

            // 如果导航框架支持后退操作，移除后退历史记录
            if (_frame.CanGoBack) _frame.RemoveBackEntry();
        };

        // 启动淡出动画
        _frame.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
    }

    /// <summary>
    /// 应用淡入动画到指定内容。
    /// </summary>
    /// <param name="content">要应用淡入动画的UI元素。</param>
    /// <remarks>
    /// 实现步骤：
    /// 1. 检查传入的UI元素是否为空，如果为空则直接返回。
    /// 2. 创建淡入动画，设置动画的起始值为0，结束值为1，持续时间为0.2秒。
    /// 3. 使用Storyboard来管理动画，设置动画的目标为传入的UI元素，并指定动画属性为Opacity。
    /// 4. 启动Storyboard以执行淡入动画。
    /// </remarks>
    private static void ApplyFadeInAnimation(UIElement? content)
    {
        if (content == null) return;

        // 创建淡入动画，设置动画的起始值为0，结束值为1，持续时间为0.2秒
        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        // 使用Storyboard来管理动画，设置动画的目标为传入的UI元素，并指定动画属性为Opacity
        Storyboard.SetTarget(fadeInAnimation, content);
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);

        // 启动Storyboard以执行淡入动画
        storyboard.Begin();
    }

    /// <summary>
    /// 导航回到前一个页面，并添加页面切换动画。
    /// </summary>
    /// <remarks>
    /// 实现步骤：
    /// 1. 检查是否可以返回上一个页面，如果不能则直接返回。
    /// 2. 获取当前页面的内容，用于执行淡出动画。
    /// 3. 从页面映射字典中获取目标页面的URI。
    /// 4. 调用 <see cref="StartPageTransitionAnimation"/> 方法执行页面切换动画。
    /// 5. 更新视图信息，将当前视图名称压入前进栈，并更新当前视图名称为后退栈顶的视图名称。
    /// </remarks>
    public void GoBack()
    {
        if (!CanGoBack) return;

        // 获取当前页面的内容，用于淡出动画
        var currentContent = _frame.Content as UIElement;

        // 从页面映射字典中获取目标页面的URI
        if (!_pageMappings.TryGetValue(_backNameStack.Peek()!, out var uri)) return;

        // 调用 StartPageTransitionAnimation 方法执行页面切换动画
        StartPageTransitionAnimation(currentContent, uri);

        // 更新视图信息
        _forwardNameStack.Push(CurrentViewName());
        _currentViewName = _backNameStack.Pop();
    }

    /// <summary>
    /// 导航到下一个页面，并添加页面切换动画。
    /// </summary>
    /// <remarks>
    /// 实现步骤：
    /// 1. 检查是否可以前进到下一个页面，如果不能则直接返回。
    /// 2. 获取当前页面的内容，用于执行淡出动画。
    /// 3. 从页面映射字典中获取目标页面的URI。
    /// 4. 调用 <see cref="StartPageTransitionAnimation"/> 方法执行页面切换动画。
    /// 5. 更新视图信息，将当前视图名称压入后退栈，并更新当前视图名称为前进栈顶的视图名称。
    /// </remarks>
    public void GoForward()
    {
        if (!CanGoForward) return;

        // 获取当前页面的内容，用于淡出动画
        var currentContent = _frame.Content as UIElement;

        // 从页面映射字典中获取目标页面的URI
        if (!_pageMappings.TryGetValue(_forwardNameStack.Peek()!, out var uri)) return;

        // 调用 StartPageTransitionAnimation 方法执行页面切换动画
        StartPageTransitionAnimation(currentContent, uri);

        // 更新视图信息
        _backNameStack.Push(CurrentViewName());
        _currentViewName = _forwardNameStack.Pop();
    }
}
