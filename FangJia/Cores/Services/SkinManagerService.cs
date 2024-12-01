using FangJia.Models;
using FangJia.Models.ConfigModels;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tomlyn;

namespace FangJia.Cores.Services;

public class SkinManagerService
{

    // 当前皮肤配置对象
    private SkinConfig? _currentSkinConfig;

    // 私有构造函数，防止外部实例化
    public SkinManagerService(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<SettingChangedEvent>()
            .Subscribe(HandleSettingChanged);

    }

    private void HandleSettingChanged(SettingChangedEventArgs args)
    {
        if (args.Key == "Theme")
        {
            // 加载新的皮肤配置
            LoadSkinConfig((string)args.NewValue);


            Window w = Application.Current.MainWindow!;

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateColorAnimations(w);
            });




        }
    }

    // 加载皮肤配置
    public void LoadSkinConfig(string skinName)
    {
        // 从 TOML 文件加载配置
        var skinConfig = LoadSkinFromFile(skinName);
        _currentSkinConfig = skinConfig;
        ApplySkin();
    }

    // 从文件加载皮肤配置
    private static SkinConfig LoadSkinFromFile(string skinName)
    {
        var filePath = $"Configs/Themes/{skinName}.toml";
        var tomlContent = File.ReadAllText(filePath);

        return new SkinConfig(Toml.Parse(tomlContent).ToModel());
    }

    // 应用皮肤到资源字典
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

    // 更新资源字典中的皮肤颜色
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
                skinResources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorValue)).Color;
            }
        }
    }
    // 遍历窗口中的所有元素，查找并更新所有ColorAnimation的目标值
    public static void UpdateColorAnimations(DependencyObject root)
    {
        // 检查该元素是否是Storyboard
        if (root is Storyboard storyboard)
        {
            // 遍历Storyboard中的所有子动画
            foreach (var child in storyboard.Children)
            {
                if (child is ColorAnimation colorAnimation)
                {
                    // 获取ColorAnimation的目标资源键
                    string resourceKey = colorAnimation.To.ToString()!; // 例如获取 "Red" 或资源名

                    // 从当前资源字典中获取新的颜色值
                    if (Application.Current.Resources[resourceKey] is Color newColor)
                    {
                        // 更新动画的目标值
                        colorAnimation.To = newColor;
                    }
                }
            }
        }

        // 递归遍历所有子元素
        int childrenCount = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            UpdateColorAnimations(child);  // 递归调用
        }
    }
}