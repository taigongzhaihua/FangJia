using FangJia.BusinessLogic.Models.Config;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FangJia.UI.Views.Components.Controls;

/// <summary>
/// SettingGroup.xaml 的交互逻辑
/// </summary>
public partial class SettingGroup
{
	public SettingGroup()
	{
		InitializeComponent();
		GroupBox.SetBinding(HeaderedContentControl.HeaderProperty, new Binding(nameof(Title)) { Source = this });
		Items.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ItemsSource)) { Source   = this });
	}

	public static readonly DependencyProperty TitleProperty =
		DependencyProperty.Register(nameof(Title),
		                            typeof(string),
		                            typeof(SettingGroup),
		                            new FrameworkPropertyMetadata(default(string)));

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public static readonly DependencyProperty ItemsSourceProperty =
		DependencyProperty.Register(nameof(ItemsSource),
		                            typeof(List<Item>),
		                            typeof(SettingGroup),
		                            new FrameworkPropertyMetadata(new List<Item>()));

	public List<Item> ItemsSource
	{
		get => (List<Item>)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}
}
