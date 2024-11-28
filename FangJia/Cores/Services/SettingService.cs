using FangJia.Models;

namespace FangJia.Cores.Services;

public class SettingService(IEventAggregator eventAggregator)
{
    public void UpdateSetting(string key, object newValue)
    {
        // 将更新存储到设置中
        // 假设你有一个方法 `SaveSetting` 来存储设置
        SaveSetting(key, newValue);

        // 发布更新事件
        eventAggregator.GetEvent<SettingChangedEvent>().Publish(new SettingChangedEventArgs
        {
            Key = key,
            NewValue = newValue
        });
    }

    private static void SaveSetting(string key, object newValue)
    {
        Properties.Settings.Default[key] = newValue;
        Properties.Settings.Default.Save();
    }

    public object GetSettingValue(string key)
    {
        // 从设置中获取值
        // 假设你有一个方法 `GetSetting` 来获取设置
        return Properties.Settings.Default[key];
    }
}