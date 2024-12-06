using FangJia.BusinessLogic.Models;
using FangJia.BusinessLogic.Models.Config;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Tomlyn;

namespace FangJia.BusinessLogic.Services;

public class SkinManagerService
{

    // 当前皮肤配置对象
    private SkinConfig? _currentSkinConfig;

    /// <summary>
    /// 初始化皮肤管理服务。
    /// </summary>
    /// <param name="eventAggregator"></param>
    public SkinManagerService(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<SettingChangedEvent>()
            .Subscribe(HandleSettingChanged);
    }

    /// <summary>
    /// 处理设置更改事件。
    /// </summary>
    /// <param name="args"></param>
    private void HandleSettingChanged(SettingChangedEventArgs args)
    {
        if (args.Key != "Theme") return;
        // 加载新的皮肤配置
        LoadSkinConfig((string)args.NewValue);
        // 获取当前应用程序的路径
        PipeService.RestartApp();
    }

    /// <summary>
    /// 加载指定名称的皮肤配置。
    /// </summary>
    /// <param name="skinName"></param>
    public void LoadSkinConfig(string skinName)
    {
        // 从 TOML 文件加载配置
        var skinConfig = LoadSkinFromFile(skinName);
        _currentSkinConfig = skinConfig;
        ApplySkin();
    }

    /// <summary>
    /// 从文件加载皮肤配置。
    /// </summary>
    /// <param name="skinName"></param>
    /// <returns></returns>
    private static SkinConfig LoadSkinFromFile(string skinName)
    {
        var filePath = $"Resources/Configs/Themes/{skinName}.toml";
        var tomlContent = File.ReadAllText(filePath);

        return new SkinConfig(Toml.Parse(tomlContent).ToModel());
    }

    /// <summary>
    /// 应用皮肤配置。
    /// </summary>
    private void ApplySkin()
    {
        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        // 找到已经加载的皮肤资源字典
        var existingSkinDictionary = mergedDictionaries.FirstOrDefault(dict =>
            dict.Source != null && dict.Source.ToString().Contains("SkinColors.xaml"));

        // 如果皮肤资源字典存在，则更新颜色资源
        if (existingSkinDictionary != null)
        {
            UpdateSkinResources(existingSkinDictionary);
        }
        else
        {
            // 如果皮肤资源字典不存在，创建新的皮肤资源字典
            var newSkinResources = new ResourceDictionary();
            UpdateSkinResources(newSkinResources);
            mergedDictionaries.Add(newSkinResources);
        }
    }

    /// <summary>
    /// 更新皮肤资源。
    /// </summary>
    /// <param name="skinResources"></param>
    private void UpdateSkinResources(ResourceDictionary skinResources)
    {
        if (_currentSkinConfig == null) return;

        // 使用反射遍历 _currentSkinConfig 的所有属性
        foreach (var property in typeof(SkinConfig).GetProperties())
        {
            // 获取属性名和属性值
            var key = property.Name;
            var colorValue = property.GetValue(_currentSkinConfig)?.ToString();

            // 使用 string.IsNullOrEmpty 检查属性值是否有效
            if (!string.IsNullOrEmpty(colorValue))
            {
                // 如果属性值有效，将其转换为 SolidColorBrush 并添加到资源字典中
                skinResources[key] = (Color)ColorConverter.ConvertFromString(colorValue);
            }
        }
    }
}
