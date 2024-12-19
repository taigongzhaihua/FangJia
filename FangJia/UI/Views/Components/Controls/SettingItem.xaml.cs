using FangJia.BusinessLogic.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using FangJia.UI.Converters;

namespace FangJia.UI.Views.Components.Controls;

/// <summary>
/// SettingItem.xaml 的交互逻辑
/// </summary>
public partial class SettingItem
{
	private readonly SettingService _settingService;
	private          UIElement      _control;

	public SettingItem()
	{
		InitializeComponent();
		SetBindings();
		_settingService = ServiceLocator.GetService<SettingService>();
		_control        = new UIElement();
	}

	private void SetBindings()
	{
		TitleBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(Title)) { Source = this });
		TipBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(Tip)) { Source     = this });
	}

	private void CreateControl()
	{
		switch (ControlType)
		{
			case "TextBox":
				var textBox = new TextBox();
				textBox.SetBinding(TextBox.TextProperty, new Binding(nameof(Value)) { Source = this });
				textBox.TextChanged += (_, _) => _settingService.UpdateSetting(Key, Value, Type.GetType(ValueType)!);
				_control            =  textBox;
				break;

			case "ComboBox":
				var comboBox = new ComboBox()
				               {
					               Height              = 24,
					               HorizontalAlignment = HorizontalAlignment.Right,
					               VerticalAlignment   = VerticalAlignment.Center
				               };
				comboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Options)) { Source = this });
				comboBox.SetBinding(Selector.SelectedItemProperty,    new Binding(nameof(Value)) { Source   = this });
				comboBox.SetBinding(StyleProperty,
				                    new Binding(nameof(ControlStyle))
				                    {
					                    Source    = this,
					                    Converter = new StringToStyleConverter()
				                    });
				comboBox.SetBinding(ComboBox.IsEnabledProperty, new Binding(nameof(IsControlEnable)) { Source = this });
				comboBox.SelectionChanged +=
					(_, _) =>
					{
						if (Value == SettingService.GetSettingValue(Key)) return;
						_settingService.UpdateSetting(Key, Value, Type.GetType(ValueType)!);
					};
				_control = comboBox;
				break;

			case "CheckBox":
				var checkBox = new CheckBox();
				checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(Value)) { Source = this });
				checkBox.Checked   += (_, _) => _settingService.UpdateSetting(Key, true,  Type.GetType(ValueType)!);
				checkBox.Unchecked += (_, _) => _settingService.UpdateSetting(Key, false, Type.GetType(ValueType)!);
				_control           =  checkBox;
				break;
		}
	}


	public static readonly DependencyProperty TitleProperty =
		DependencyProperty.Register(nameof(Title),
		                            typeof(string),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata(default(string)));

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public static readonly DependencyProperty ControlTypeProperty =
		DependencyProperty.Register(nameof(ControlType),
		                            typeof(string),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata
			                            (null,
			                             FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
			                             OnValueChanged));

	private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not SettingItem settingItem) return;
		settingItem.CreateControl();
		Grid.SetRow(settingItem._control, 0);
		Grid.SetColumn(settingItem._control, 1);
		settingItem.Grid.Children.Add(settingItem._control);
	}

	public string ControlType
	{
		get => (string)GetValue(ControlTypeProperty);
		set => SetValue(ControlTypeProperty, value);
	}

	public static readonly DependencyProperty TipProperty =
		DependencyProperty.Register(nameof(Tip),
		                            typeof(string),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata(default(string)));

	public string Tip
	{
		get => (string)GetValue(TipProperty);
		set => SetValue(TipProperty, value);
	}

	public static readonly DependencyProperty OptionsProperty =
		DependencyProperty.Register(nameof(Options),
		                            typeof(List<string>),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata(default(List<string>)));

	public List<string> Options
	{
		get => (List<string>)GetValue(OptionsProperty);
		set => SetValue(OptionsProperty, value);
	}

	public static readonly DependencyProperty KeyProperty =
		DependencyProperty.Register(nameof(Key),
		                            typeof(string),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata
			                            (null,
			                             FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
			                             OnKeyChanged));

	private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not SettingItem settingItem) return;
		settingItem.Value = SettingService.GetSettingValue(settingItem.Key);
	}

	public string Key
	{
		get => (string)GetValue(KeyProperty);
		set => SetValue(KeyProperty, value);
	}

	public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register(nameof(Value),
		                            typeof(object),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata(default(object)));

	public object Value
	{
		get => GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public static readonly DependencyProperty ValueTypeProperty =
		DependencyProperty.Register(nameof(ValueType),
		                            typeof(string),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata(default(string)));

	public string ValueType
	{
		get => (string)GetValue(ValueTypeProperty);
		set => SetValue(ValueTypeProperty, value);
	}

	public static readonly DependencyProperty ControlStyleProperty =
		DependencyProperty.Register(nameof(ControlStyle),
		                            typeof(string),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata(default(string)));

	public string ControlStyle
	{
		get => (string)GetValue(ControlStyleProperty);
		set => SetValue(ControlStyleProperty, value);
	}

	public static readonly DependencyProperty IsControlEnableProperty =
		DependencyProperty.Register(nameof(IsControlEnable),
		                            typeof(bool),
		                            typeof(SettingItem),
		                            new FrameworkPropertyMetadata(true));

	public bool IsControlEnable
	{
		get => (bool)GetValue(IsControlEnableProperty);
		set => SetValue(IsControlEnableProperty, value);
	}
}
