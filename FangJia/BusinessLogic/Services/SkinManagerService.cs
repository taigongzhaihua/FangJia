using FangJia.BusinessLogic.Models;
using FangJia.BusinessLogic.Models.Config;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Tomlyn;

namespace FangJia.BusinessLogic.Services;

/// <summary>
/// 皮肤管理服务，负责加载和应用应用程序的皮肤配置。
/// </summary>
public class SkinManagerService
{
    // 当前皮肤配置对象
    private SkinConfig? _currentSkinConfig;

    /// <summary>
    /// 初始化皮肤管理服务。
    /// </summary>
    /// <param name="eventAggregator">事件聚合器，用于订阅设置更改事件。</param>
    public SkinManagerService(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<SettingChangedEvent>()
            .Subscribe(HandleSettingChanged);
    }

    /// <summary>
    /// 处理设置更改事件。
    /// </summary>
    /// <param name="args">设置更改事件参数。</param>
    private void HandleSettingChanged(SettingChangedEventArgs args)
    {
        // 检查事件是否与主题更改相关
        if (args.Key != "Theme") return;
        // 加载新的皮肤配置
        LoadSkinConfig((string)args.NewValue);

        // 重启应用程序以应用新的皮肤配置
        PipeService.RestartApp();
    }

    /// <summary>
    /// 加载指定名称的皮肤配置。
    /// </summary>
    /// <param name="skinName">皮肤配置的名称。</param>
    public void LoadSkinConfig(string skinName)
    {
        // 从 TOML 文件加载皮肤配置
        var skinConfig = LoadSkinFromFile(skinName);
        _currentSkinConfig = skinConfig;

        // 应用加载的皮肤配置
        ApplySkin();
    }

    /// <summary>
    /// 从文件加载皮肤配置。
    /// </summary>
    /// <param name="skinName">皮肤配置的名称。</param>
    /// <returns>加载的皮肤配置对象。</returns>
    private static SkinConfig LoadSkinFromFile(string skinName)
    {
        // 构建皮肤配置文件的路径
        string filePath = $"Resources/Configs/Themes/{skinName}.toml";

        // 读取 TOML 文件内容
        var tomlContent = File.ReadAllText(filePath);

        // 解析 TOML 内容并转换为皮肤配置对象
        return new SkinConfig(Toml.Parse(tomlContent).ToModel());
    }

    /// <summary>
    /// 应用皮肤配置。
    /// </summary>
    private void ApplySkin()
    {
        // 获取当前应用程序的资源字典
        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        // 查找已经加载的皮肤资源字典
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
    /// <param name="skinResources">要更新的资源字典。</param>
    private void UpdateSkinResources(ResourceDictionary skinResources)
    {
        // 检查当前皮肤配置是否为空
        if (_currentSkinConfig == null) return;

        // 使用反射遍历 _currentSkinConfig 的所有属性
        foreach (var property in typeof(SkinConfig).GetProperties())
        {
            // 获取属性名和属性值
            var key = property.Name;
            var colorValue = property.GetValue(_currentSkinConfig)?.ToString();

            // 检查属性值是否有效
            if (!string.IsNullOrEmpty(colorValue))
            {
                // 将属性值转换为 SolidColorBrush 并添加到资源字典中
                skinResources[key] = (Color)ColorConverter.ConvertFromString(colorValue);
            }
        }
    }
}
