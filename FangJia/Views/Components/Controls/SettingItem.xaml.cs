using FangJia.Cores.Services;
using NLog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace FangJia.Views.Components.Controls;

/// <summary>
/// SettingItem.xaml 的交互逻辑
/// </summary>
public partial class SettingItem
{
    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SettingService _settingService;
    private UIElement _control;
    public SettingItem()
    {
        InitializeComponent();
        SetBindings();
        IEventAggregator eventAggregator = new EventAggregator();
        _settingService = new SettingService(eventAggregator);
        _control = new UIElement();
    }

    private void SetBindings()
    {
        TitleBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(Title)) { Source = this });
        TipBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(Tip)) { Source = this });
    }
    private void CreateControl()
    {
        switch (ControlType)
        {
            case "TextBox":
                var textBox = new TextBox();
                textBox.SetBinding(TextBox.TextProperty, new Binding(nameof(Value)) { Source = this });
                textBox.TextChanged += (_, _) =>
                {
                    _settingService.UpdateSetting(Key, Value);
                };
                _control = textBox;
                break;

            case "ComboBox":
                var comboBox = new ComboBox();
                comboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Options)) { Source = this });
                comboBox.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(Value)) { Source = this });
                comboBox.SelectionChanged += (_, _) =>
                {
                    _settingService.UpdateSetting(Key, Value);
                    Logger.Debug($"key = {Key}, value = {Value}");
                };
                _control = comboBox;
                break;

            case "CheckBox":
                var checkBox = new CheckBox();
                checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(Value)) { Source = this });
                checkBox.Checked += (_, _) =>
                {
                    _settingService.UpdateSetting(Key, true);
                };
                checkBox.Unchecked += (_, _) =>
                {
                    _settingService.UpdateSetting(Key, false);
                };
                _control = checkBox;
                break;
        }
    }


    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                default(string)
            )
        );
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty ControlTypeProperty =
        DependencyProperty.Register(
            nameof(ControlType),
            typeof(string),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                default(string),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, _) =>
                {
                    var settingItem = (SettingItem)o;
                    settingItem.CreateControl();
                    Grid.SetRow(settingItem._control, 0);
                    Grid.SetColumn(settingItem._control, 1);
                    settingItem.Grid.Children.Add(settingItem._control);
                }
            )
        );
    public string ControlType
    {
        get => (string)GetValue(ControlTypeProperty);
        set => SetValue(ControlTypeProperty, value);
    }

    public static readonly DependencyProperty TipProperty =
        DependencyProperty.Register(
            nameof(Tip),
            typeof(string),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                default(string)
            )
        );
    public string Tip
    {
        get => (string)GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    public static readonly DependencyProperty OptionsProperty =
        DependencyProperty.Register(
            nameof(Options),
            typeof(List<string>),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                default(List<string>)
            )
        );
    public List<string> Options
    {
        get => (List<string>)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register(
            nameof(Key),
            typeof(string),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                default(string),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, _) =>
                {
                    var settingItem = (SettingItem)o;
                    settingItem.Value = (string)settingItem._settingService.GetSettingValue(settingItem.Key);
                    Logger.Debug($"key = {settingItem.Key}，value = {settingItem._settingService.GetSettingValue(settingItem.Key)}");
                }
            )
        );
    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                default(string)
            )
        );
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}