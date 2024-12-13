using FangJia.BusinessLogic.Models;

namespace FangJia.BusinessLogic.Services;

/// <summary>
/// 设置服务类，用于处理应用程序的设置更新和获取。
/// </summary>
public class SettingService(IEventAggregator eventAggregator)
{
    /// <summary>
    /// 更新设置并发布更新事件。
    /// </summary>
    /// <param name="key">设置的键。</param>
    /// <param name="newValue">设置的新值。</param>
    /// <param name="type">新值的类型。</param>
    public void UpdateSetting(string key, object newValue, Type type)
    {
        // 将更新存储到设置中
        SaveSetting(key, newValue, type);

        // 发布更新事件
        eventAggregator.GetEvent<SettingChangedEvent>().Publish(new SettingChangedEventArgs
        {
            Key = key,
            NewValue = newValue
        });
    }

    /// <summary>
    /// 将设置保存到应用程序设置中。
    /// </summary>
    /// <param name="key">设置的键。</param>
    /// <param name="newValue">设置的新值。</param>
    /// <param name="type">新值的类型。</param>
    private static void SaveSetting(string key, object newValue, Type type)
    {
        Properties.Settings.Default[key] = Convert.ChangeType(newValue, type);
        Properties.Settings.Default.Save();
    }

    /// <summary>
    /// 获取指定键的设置值。
    /// </summary>
    /// <param name="key">设置的键。</param>
    /// <returns>设置的值。</returns>
    public static object GetSettingValue(string key) => Properties.Settings.Default[key];
}
