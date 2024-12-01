using FangJia.Cores.Interfaces;
using FangJia.Models.ConfigModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FangJia.Cores.Services.NavigationServices;

public class FrameNavigationService : INavigationService
{
    private readonly Frame _frame;
    private readonly Dictionary<string, string> _pageMappings;
    private string? _currentViewName;
    private readonly Stack<string?> _backNameStack = new();
    private readonly Stack<string?> _forwardNameStack = new();

    public FrameNavigationService(Frame frame, List<PageConfig> pageConfigs)
    {
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        _pageMappings = [];

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
        if (viewName == CurrentViewName()) return;

        if (_pageMappings.TryGetValue(viewName!, out var uri))
        {
            // 获取当前页面的内容
            var currentContent = _frame.Content as UIElement;

            // 执行页面切换动画
            StartPageTransitionAnimation(currentContent, uri, "Navigate");

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

    private void StartPageTransitionAnimation(UIElement? currentContent, string uri, string type)
    {
        if (currentContent == null)
        {
            _frame.Navigate(new Uri(uri, UriKind.Relative));
            var newContent = _frame.Content as UIElement;
            ApplyFadeInAnimation(newContent);
        }
        else
        {
            // 执行当前页面的淡出动画
            ApplyFadeOutAnimation(currentContent, uri, type);
        }
    }

    private void ApplyFadeOutAnimation(UIElement content, string uri, string type)
    {
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        fadeOutAnimation.Completed += (_, _) =>
        {
            switch (type)
            {
                case "Back":
                    _frame.GoBack();
                    break;
                case "Forward":
                    _frame.GoForward();
                    break;
                case "Navigate":
                    _frame.Navigate(new Uri(uri, UriKind.Relative));
                    break;
            }

            // 在新页面加载完成后应用淡入动画
            var newContent = _frame.Content as UIElement;
            ApplyFadeInAnimation(newContent);
        };

        content.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
    }

    private static void ApplyFadeInAnimation(UIElement? content)
    {
        if (content == null) return;
        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        content.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
    }

    /// <summary>
    /// 导航回到前一个页面，并添加页面切换动画。
    /// </summary>
    public void GoBack()
    {
        if (!_frame.CanGoBack) return;

        // 获取当前页面的内容，用于淡出动画
        var currentContent = _frame.Content as UIElement;
        if (!_pageMappings.TryGetValue(_backNameStack.Peek()!, out var uri)) return;
        // 执行页面后退的动画
        StartPageTransitionAnimation(currentContent, uri, "Back");

        // 更新视图信息
        _forwardNameStack.Push(CurrentViewName());
        _currentViewName = _backNameStack.Pop();

    }

    /// <summary>
    /// 导航到下一个页面，并添加页面切换动画。
    /// </summary>
    public void GoForward()
    {
        if (!_frame.CanGoForward) return;

        // 获取当前页面的内容，用于淡出动画
        var currentContent = _frame.Content as UIElement;

        if (!_pageMappings.TryGetValue(_forwardNameStack.Peek()!, out var uri)) return;

        // 执行页面后退的动画
        StartPageTransitionAnimation(currentContent, uri, "Forward");

        // 更新视图信息
        _backNameStack.Push(CurrentViewName());
        _currentViewName = _forwardNameStack.Pop();

    }
}