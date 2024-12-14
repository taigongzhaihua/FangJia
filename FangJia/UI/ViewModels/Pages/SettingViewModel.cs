using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using FangJia.BusinessLogic.Models.Config;
using Tomlyn;
using Tomlyn.Model;

namespace FangJia.UI.ViewModels.Pages;

/// <summary>
/// SettingViewModel 类负责处理应用程序的设置页面视图模型。
/// 该类从配置文件中读取 TOML 格式的设置数据，并将其解析为 Group 和 Item 对象的集合。
/// 这些对象随后用于绑定到 UI 控件以显示和编辑设置。
/// </summary>
public partial class SettingViewModel : ObservableObject
{
	/// <summary>
	/// SettingViewModel 类的构造函数。
	/// 该构造函数负责从配置文件中读取 TOML 格式的设置数据，并将其解析为 Group 和 Item 对象的集合。
	/// 这些对象随后用于绑定到 UI 控件以显示和编辑设置。
	/// </summary>
	/// <remarks>
	/// 实现步骤如下：
	/// 1. 读取配置文件的内容。
	/// 2. 解析 TOML 内容为 TomlTable 对象。
	/// 3. 提取 "Groups" 部分的数据。
	/// 4. 遍历每个组，解析并映射数据到 C# 的 Group 类。
	/// 5. 遍历组中的每个项，解析并映射数据到 C# 的 Item 类。
	/// 6. 如果项中包含 "Options" 字段，则解析并添加到选项列表。
	/// 7. 将解析后的组添加到 Groups 集合中。
	/// </remarks>
	public SettingViewModel()
	{
		// 1. 读取配置文件的内容
		// 从资源文件中获取配置文件的路径，并读取其内容
		var tomlContent = File.ReadAllText(Properties.Resources.SettingConfigUri);

		// 2. 解析 TOML 内容为 TomlTable 对象
		// 使用 Tomlyn 库解析 TOML 内容，并将其转换为 TomlTable 对象
		var tomlTable = Toml.Parse(tomlContent).ToModel();

		// 3. 提取 "Groups" 部分的数据
		// 从解析后的 TomlTable 对象中提取 "Groups" 部分的数据，这是一个 TomlTableArray 对象
		var groups = tomlTable["Groups"] as TomlTableArray;

		// 4. 遍历每个组
		// 遍历每个组，解析并映射数据到 C# 的 Group 类
		foreach (var group in groups!)
		{
			// 4.1 解析并映射数据到 C# 的 Group 类
			var appearanceGroup = new Group
			                      {
				                      Title = group["Title"].ToString()!, // 组的标题
				                      Key   = group["Key"].ToString()!,   // 组的键
				                      Items = []                          // 组的项列表，初始为空
			                      };

			// 4.2 遍历组中的每个项
			// 遍历组中的每个项，解析并映射数据到 C# 的 Item 类
			foreach (var item in (group["Items"] as TomlTableArray)!)
			{
				// 4.2.1 解析并映射数据到 C# 的 Item 类
				var outItem = new Item
				              {
					              Name         = item["Name"].ToString()!,         // 项的名称
					              Key          = item["Key"].ToString()!,          // 项的键
					              Type         = item["Type"].ToString()!,         // 项的类型
					              ControlType  = item["ControlType"].ToString()!,  // 控件类型
					              ControlStyle = item["ControlStyle"].ToString()!, // 控件样式
					              Options      = [],                               // 选项列表，初始为空
					              IsEnable     = (bool)item["IsEnable"],           // 是否启用
					              Tip          = item["Tip"].ToString()!           // 提示信息
				              };

				// 4.2.2 如果项中包含 "Options" 字段，则解析并添加到选项列表
				if (item.TryGetValue("Options", out var options))
				{
					foreach (var option in (options as TomlArray)!)
					{
						outItem.Options.Add(option?.ToString()!);
					}
				}

				// 4.2.3 将解析后的项添加到组的项列表中
				appearanceGroup.Items.Add(outItem);
			}

			// 4.3 将解析后的组添加到 Groups 集合中
			Groups.Add(appearanceGroup);
		}
	}

	/// <summary>
	/// 使用 ObservableProperty 特性标记的属性，用于绑定 UI 控件。
	/// 该属性是一个 ObservableCollection&lt;Group&gt; 类型的集合，用于存储从配置文件中解析出来的组对象。
	/// </summary>
	[ObservableProperty] private ObservableCollection<Group> _groups = [];
}
