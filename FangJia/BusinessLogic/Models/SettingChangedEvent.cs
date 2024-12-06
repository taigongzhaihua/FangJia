namespace FangJia.BusinessLogic.Models;

public class SettingChangedEvent : PubSubEvent<SettingChangedEventArgs> { }

public class SettingChangedEventArgs
{
    public string? Key { get; set; }
    public required object NewValue { get; set; }
}