using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace FangJia.Cores.Services;

/// <summary>
/// 配置服务类，用于管理应用程序的配置文件。
/// 提供读取、修改和添加配置项的功能。
/// </summary>
/// <remarks>
/// 配置文件路径。
/// </remarks>
/// <param name="configFilePath">配置文件路径。</param>
public sealed class ConfigurationService(string configFilePath)
{
    // 配置文件路径，默认为当前目录下的 AppOptions.json 文件

    /// <summary>
    /// 从配置文件中读取所有配置选项。
    /// </summary>
    /// <returns>包含所有配置项的字典，键为配置名称，值为配置值列表。</returns>
    public Dictionary<string, List<T>> LoadConfigs<T>()
    {
        // 读取配置文件的内容
        var jsonContent = File.ReadAllText(configFilePath);

        // 解析 JSON 内容，定位到 {configName} 节点
        var jToken = JObject.Parse(jsonContent);

        // 将 {configName} 节点转换为字典类型，若为空则返回空字典
        return jToken?.ToObject<Dictionary<string, List<T>>>() ?? [];
    }

    /// <summary>
    /// 从配置文件中获取页面配置列表。
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="key">配置项名称</param>
    /// <returns>配置项列表</returns>
    public List<T> GetConfig<T>(string key)
    {
        // 解析 JSON 内容
        var jObject = LoadConfigs<T>();

        // 查找指定键的配置
        return jObject.TryGetValue(key, out var config) ? config : [];
    }

    /// <summary>
    /// 更新指定配置项的值。
    /// 如果配置项存在，则覆盖其值；否则新增配置项。
    /// </summary>
    /// <param name="key">配置项的名称。</param>
    /// <param name="newValues">配置项的新值列表。</param>
    public void UpdateOption(string key, List<string> newValues)
    {
        // 读取配置文件的内容并解析为 JSON 对象
        var jsonContent = File.ReadAllText(configFilePath);
        var jsonObject = JObject.Parse(jsonContent);

        // 定位到 "Options" 节点，并更新指定键的值
        jsonObject["Options"]![key] = JArray.FromObject(newValues);

        // 将更新后的 JSON 对象写回配置文件
        File.WriteAllText(configFilePath, jsonObject.ToString(Formatting.Indented));
    }

    /// <summary>
    /// 添加新的配置项。
    /// 如果配置项已存在，则不进行任何操作。
    /// </summary>
    /// <param name="key">新配置项的名称。</param>
    /// <param name="values">新配置项的值列表。</param>
    public void AddOption(string key, List<string> values)
    {
        // 读取配置文件的内容并解析为 JSON 对象
        var jsonContent = File.ReadAllText(configFilePath);
        var jsonObject = JObject.Parse(jsonContent);

        // 检查 "Options" 节点下是否已存在指定键
        if (jsonObject["Options"]![key] != null) return;

        // 添加新的键值对到 "Options" 节点
        jsonObject["Options"]![key] = JArray.FromObject(values);

        // 将更新后的 JSON 对象写回配置文件
        File.WriteAllText(configFilePath, jsonObject.ToString(Formatting.Indented));
    }
}