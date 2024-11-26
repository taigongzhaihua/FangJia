using FangJia.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FangJia.Views.Components.Controls;

/// <summary>
/// MainMenu.xaml 的交互逻辑
/// </summary>
public partial class MainMenu
{

    public MainMenu()
    {
        InitializeComponent();
        VisualStateManager.GoToState(this, "Close", false);
    }

    public static readonly DependencyProperty MenuItemsProperty =
        DependencyProperty.Register(
            nameof(MenuItems),
            typeof(ObservableCollection<MainMenuItemData>),
            typeof(MainMenu),
            new PropertyMetadata(
                new ObservableCollection<MainMenuItemData>()
                )
            );

    public ObservableCollection<MainMenuItemData> MenuItems
    {
        get => (ObservableCollection<MainMenuItemData>)GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 如果用户尝试取消选中，恢复到上一个选中项
        if (sender is ListBox { SelectedItem: null, Items.Count: > 0 } listBox)
        {
            listBox.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : listBox.Items[0];
        }
    }

    private bool _isOpened;

    public bool IsOpen
    {
        get => _isOpened;
        set => SetProperty(ref _isOpened, value);
    }
    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
        VisualStateManager.GoToState(this, IsOpen ? "Open" : "Close", true);
    }

    private void EventSetter_OnHandler(object sender, MouseEventArgs e)
    {
        if (sender is ListBoxItem { IsSelected: false } && IsOpen == false)
        {
            VisualStateManager.GoToState(this, "Open", true);
        }
    }

    private void EventSetter_OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is ListBoxItem && IsOpen == false)
        {
            VisualStateManager.GoToState(this, "Close", true);
        }
    }
}