using FangJia.BusinessLogic.Models;

namespace FangJia.BusinessLogic.Services;

public class SettingService(IEventAggregator eventAggregator)
{
    public void UpdateSetting(string key, object newValue, Type type)
    {
        // 将更新存储到设置中
        // 假设你有一个方法 `SaveSetting` 来存储设置
        SaveSetting(key, newValue, type);

        // 发布更新事件
        eventAggregator.GetEvent<SettingChangedEvent>().Publish(new SettingChangedEventArgs
        {
            Key = key,
            NewValue = newValue
        });
    }

    private static void SaveSetting(string key, object newValue, Type type)
    {
        Properties.Settings.Default[key] = Convert.ChangeType(newValue, type);
        Properties.Settings.Default.Save();
    }

    public static object GetSettingValue(string key) => Properties.Settings.Default[key];
}