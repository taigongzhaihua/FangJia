using FangJia.BusinessLogic.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace FangJia.UI.Views.Components.Controls;

/// <summary>
/// MainMenu.xaml 的交互逻辑
/// </summary>
public partial class MainMenu
{
    public MainMenu()
    {
        InitializeComponent();
        VisualStateManager.GoToState(this, "Close", false);
        TitleBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(Title)) { Source = this });
        SidebarListBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(MenuItems)) { Source = this });
        SidebarListBox.SetBinding(Selector.SelectedIndexProperty, new Binding(nameof(MenuSelectedIndex)) { Source = this });
    }

    public static readonly DependencyProperty MenuItemsProperty =
        DependencyProperty.Register(
            nameof(MenuItems),
            typeof(ObservableCollection<MainMenuItem>),
            typeof(MainMenu),
            new FrameworkPropertyMetadata(
                new ObservableCollection<MainMenuItem>()
                )
            );

    public ObservableCollection<MainMenuItem> MenuItems
    {
        get => (ObservableCollection<MainMenuItem>)GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(MainMenu),
            new FrameworkPropertyMetadata(
                default(string)
                )
            );

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MenuSelectedIndexProperty = DependencyProperty.Register(
        nameof(MenuSelectedIndex), typeof(int), typeof(MainMenu), new FrameworkPropertyMetadata(0));

    public int MenuSelectedIndex
    {
        get => (int)GetValue(MenuSelectedIndexProperty);
        set => SetValue(MenuSelectedIndexProperty, value);
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
}