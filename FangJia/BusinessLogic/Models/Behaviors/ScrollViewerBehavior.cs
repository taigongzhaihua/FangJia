using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FangJia.BusinessLogic.Models.Behaviors;

public static class ScrollViewerBehavior
{
    // 附加属性，用于启用或禁用行为
    public static readonly DependencyProperty AutoHideScrollBarProperty =
        DependencyProperty.RegisterAttached(
                                            "AutoHideScrollBar",
                                            typeof(bool),
                                            typeof(ScrollViewerBehavior),
                                            new PropertyMetadata(false, OnAutoHideScrollBarChanged));

    public static bool GetAutoHideScrollBar(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoHideScrollBarProperty);
    }

    public static void SetAutoHideScrollBar(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoHideScrollBarProperty, value);
    }

    // 属性值变化时调用
    private static void OnAutoHideScrollBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            if ((bool)e.NewValue)
            {
                // 添加事件处理
                scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            }
            else
            {
                // 移除事件处理
                scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }
    }

    // 鼠标滚轮事件处理逻辑
    private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            // 滚轮滚动时显示滚动条
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            // 设置延迟隐藏滚动条
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, args) =>
                          {
                              scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                              timer.Stop();
                          };
            timer.Start();
        }
    }
}
