using FangJia.Models.ConfigModels;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Tomlyn;

namespace FangJia.Cores.Services
{
    public class SkinManagerService
    {
        // 使用 Lazy<T> 来实现单例模式
        private static readonly Lazy<SkinManagerService> _instance = new(() => new SkinManagerService(), isThreadSafe: true);
        public static SkinManagerService Instance => _instance.Value;

        // 当前皮肤配置对象
        private SkinConfig? _currentSkinConfig;

        // 私有构造函数，防止外部实例化
        private SkinManagerService()
        {
            // 默认加载一个皮肤配置
            LoadSkinConfig("lightTheme");
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
        private static SkinConfig? LoadSkinFromFile(string skinName)
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
    }
}

