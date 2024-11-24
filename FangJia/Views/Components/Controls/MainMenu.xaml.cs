using FangJia.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace FangJia.Views.Components.Controls;

/// <summary>
/// MainMenu.xaml 的交互逻辑
/// </summary>
public partial class MainMenu
{

    public MainMenu()
    {
        InitializeComponent();
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
}