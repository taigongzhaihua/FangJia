using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FangJia.DataAccess;

/// <summary>
/// 配置服务类，用于管理应用程序的配置文件。
/// 提供读取、修改和添加配置项的功能。
/// </summary>
/// <remarks>
/// 配置文件路径由构造函数参数提供。
/// </remarks>
/// <param name="configFilePath">配置文件路径。</param>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class ConfigurationService(string configFilePath)
{
    /// <summary>
    /// 从配置文件中读取所有配置项。
    /// </summary>
    /// <typeparam name="T">配置项值的类型。</typeparam>
    /// <returns>包含所有配置项的字典，键为配置名称，值为配置项值的列表。</returns>
    public Dictionary<string, List<T>> LoadConfigs<T>()
    {
        // 读取配置文件内容
        var jsonContent = File.ReadAllText(path: configFilePath);

        // 解析 JSON 内容为 JObject
        var jToken = JObject.Parse(json: jsonContent);

        // 将 JObject 转换为字典，若为空则返回空字典
        return jToken.ToObject<Dictionary<string, List<T>>>() ?? [];
    }

    /// <summary>
    /// 根据指定的键从配置文件中获取配置项列表。
    /// </summary>
    /// <typeparam name="T">配置项值的类型。</typeparam>
    /// <param name="key">配置项的名称。</param>
    /// <returns>指定键的配置项列表，如果键不存在则返回空列表。</returns>
    public List<T> GetConfig<T>(string key)
    {
        // 获取所有配置项
        var jObject = LoadConfigs<T>();

        // 查找并返回指定键的配置项列表
        return jObject.TryGetValue(key: key, value: out var config) ? config : [];
    }

    /// <summary>
    /// 更新或新增指定的配置项。
    /// 如果配置项已存在，则覆盖其值；否则新增该配置项。
    /// </summary>
    /// <typeparam name="T">配置项值的类型。</typeparam>
    /// <param name="key">配置项的名称。</param>
    /// <param name="newValues">配置项的新值列表。</param>
    public void UpdateOption<T>(string key, List<T> newValues)
    {
        // 获取所有配置项
        var configs = LoadConfigs<T>();

        // 更新或新增指定键的配置项值
        configs[key] = newValues;

        // 保存更新后的配置项
        SaveConfigs(configs);
    }

    /// <summary>
    /// 添加新的配置项。
    /// 如果配置项已存在，则不进行任何操作。
    /// </summary>
    /// <typeparam name="T">配置项值的类型。</typeparam>
    /// <param name="key">新配置项的名称。</param>
    /// <param name="values">新配置项的值列表。</param>
    public void AddOption<T>(string key, List<T> values)
    {
        // 获取所有配置项
        var configs = LoadConfigs<T>();

        // 检查是否已存在指定键，若存在则返回
        if (!configs.TryAdd(key, values)) return;

        // 保存更新后的配置项
        SaveConfigs(configs);
    }

    /// <summary>
    /// 将配置项保存到配置文件中。
    /// </summary>
    /// <typeparam name="T">配置项值的类型。</typeparam>
    /// <param name="configs">要保存的配置项字典。</param>
    private void SaveConfigs<T>(Dictionary<string, List<T>> configs)
    {
        // 将配置项字典转换为 JSON 对象
        var jsonObject = JObject.FromObject(configs);

        // 将 JSON 对象写入配置文件
        File.WriteAllText(path: configFilePath, contents: jsonObject.ToString(formatting: Formatting.Indented));
    }
}
