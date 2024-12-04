using FangJia.Cores.Interfaces;
using FangJia.Models.ConfigModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FangJia.Cores.Services.NavigationServices;

public class FrameNavigationService : INavigationService
{
    private Frame _frame = null!;
    private readonly Dictionary<string, string> _pageMappings = [];
    private string? _currentViewName;
    private readonly Stack<string?> _backNameStack = new();
    private readonly Stack<string?> _forwardNameStack = new();
    public bool CanGoBack => _backNameStack.Count > 0;
    public bool CanGoForward => _forwardNameStack.Count > 0;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public void SetPageMappings(List<PageConfig> pageConfigs)
    {
        _pageMappings.Clear();
        foreach (var config in pageConfigs)
        {
            _pageMappings[config.Name!] = config.Uri!;
        }
    }

    public string? CurrentViewName()
    {
        return _currentViewName;
    }

    public void NavigateTo(string? viewName)
    {
        if (viewName == CurrentViewName() && _frame.Content != null) return;

        if (_pageMappings.TryGetValue(viewName!, out var uri))
        {
            // 获取当前页面的内容
            var currentContent = _frame.Content as UIElement;

            // 执行页面切换动画
            StartPageTransitionAnimation(currentContent, uri);

            // 更新视图信息
            if (!string.IsNullOrEmpty(_currentViewName))
            {
                _backNameStack.Push(CurrentViewName());
                _forwardNameStack.Clear();
            }
            _currentViewName = viewName!;
        }
        else
        {
            throw new ArgumentException($"View {viewName} not found in configuration.");
        }
    }

    private void StartPageTransitionAnimation(UIElement? currentContent, string uri)
    {
        if (currentContent == null)
        {
            _frame.Navigate(new Uri(uri, UriKind.Relative));

            ApplyFadeInAnimation(_frame);  // 应用淡入动画
        }
        else
        {
            // 执行当前页面的淡出动画
            ApplyFadeOutAnimation(uri);
        }
    }

    private void ApplyFadeOutAnimation(string uri)
    {
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        fadeOutAnimation.Completed += (_, _) =>
        {

            _frame.Navigate(new Uri(uri, UriKind.Relative));


            // 在新页面加载完成后应用淡入动画
            ApplyFadeInAnimation(_frame);  // 应用淡入动画

            if (_frame.CanGoBack) _frame.RemoveBackEntry();
        };

        _frame.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
    }

    private static void ApplyFadeInAnimation(UIElement? content)
    {
        if (content == null) return;

        // 通过 Storyboard 执行渐变动画，确保页面加载后才淡入
        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.2)  // 设置合适的持续时间
        };
        Storyboard.SetTarget(fadeInAnimation, content);
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Begin();
    }

    /// <summary>
    /// 导航回到前一个页面，并添加页面切换动画。
    /// </summary>
    public void GoBack()
    {
        if (!CanGoBack) return;

        // 获取当前页面的内容，用于淡出动画
        var currentContent = _frame.Content as UIElement;
        if (!_pageMappings.TryGetValue(_backNameStack.Peek()!, out var uri)) return;
        // 执行页面后退的动画
        StartPageTransitionAnimation(currentContent, uri);

        // 更新视图信息
        _forwardNameStack.Push(CurrentViewName());
        _currentViewName = _backNameStack.Pop();

    }

    /// <summary>
    /// 导航到下一个页面，并添加页面切换动画。
    /// </summary>
    public void GoForward()
    {
        if (!CanGoForward) return;

        // 获取当前页面的内容，用于淡出动画
        var currentContent = _frame.Content as UIElement;

        if (!_pageMappings.TryGetValue(_forwardNameStack.Peek()!, out var uri)) return;

        // 执行页面后退的动画
        StartPageTransitionAnimation(currentContent, uri);

        // 更新视图信息
        _backNameStack.Push(CurrentViewName());
        _currentViewName = _forwardNameStack.Pop();

    }
}