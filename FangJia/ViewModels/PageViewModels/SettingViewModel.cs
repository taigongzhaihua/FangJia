using FangJia.Cores.Base;
using FangJia.Models.ConfigModels;
using NLog;
using System.Collections.ObjectModel;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace FangJia.ViewModels.PageViewModels;

public partial class SettingViewModel : BaseViewModel
{
    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public SettingViewModel()
    {
        // 读取配置文件
        var tomlContent = File.ReadAllText("Configs/SettingConfig.toml");

        // 解析 TOML 内容
        var tomlTable = Toml.Parse(tomlContent).ToModel();

        // 提取 "Groups" 部分的数据
        var groups = tomlTable["Groups"] as TomlTableArray;
        foreach (var group in groups!)
        {
            // 解析并映射数据到 C# 类
            var appearanceGroup = new Group
            {
                Title = group["Title"].ToString()!,
                Key = group["Key"].ToString()!,
                Items = []
            };

            foreach (var item in (group["Items"] as TomlTableArray)!)
            {
                var outItem = new Item
                {
                    Name = item["Name"].ToString()!,
                    Key = item["Key"].ToString()!,
                    Type = item["Type"].ToString()!,
                    ControlType = item["ControlType"].ToString()!,
                    ControlStyle = item["ControlStyle"].ToString()!,
                    Options = [],
                    IsEnable = (bool)item["IsEnable"],
                    Tip = item["Tip"].ToString()!
                };
                if (item.TryGetValue("Options", out var options))
                {
                    foreach (var option in (options as TomlArray)!)
                    {
                        outItem.Options.Add(option?.ToString()!);
                    }
                }
                appearanceGroup.Items.Add(outItem);
            }
            Groups.Add(appearanceGroup);
        }
        Logger.Debug(Groups.Count);
        Logger.Debug(Groups[0].Title);
    }

    public ObservableCollection<Group> Groups
    {
        get; set;
    } = [];
}