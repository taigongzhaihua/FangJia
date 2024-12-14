// 皮肤配置类，存储与应用程序皮肤相关的颜色信息

using Tomlyn.Model;

namespace FangJia.BusinessLogic.Models.Config;

public class SkinConfig
{
	public string? BackgroundColor                { get; set; } // 背景颜色
	public string? ForegroundColor                { get; set; } // 前景颜色（通常是文本颜色）
	public string? AccentBackgroundColor          { get; set; } // 强调背景颜色，通常用于按钮、菜单或重要元素
	public string? AccentForegroundColor          { get; set; } // 强调前景颜色，通常用于按钮上的文本或图标
	public string? HoverOverlayColor              { get; set; } // 悬停时的叠加颜色，用于鼠标悬停的状态
	public string? PressedOverlayColor            { get; set; } // 按钮或元素按下时的叠加颜色
	public string? AccentOverlayColor             { get; set; } // 强调叠加颜色，通常用于高亮显示
	public string? SwitchOnColor                  { get; set; } // 开关开启时的颜色
	public string? SwitchOffColor                 { get; set; } // 开关关闭时的颜色
	public string? ShadowColor                    { get; set; } // 阴影颜色，用于创造深度效果
	public string? WeakAccentBackgroundColor      { get; set; } // 弱强调背景色，通常用于非关键元素的背景颜色
	public string? WeakAccentForegroundColor      { get; set; } // 弱强调前景色，通常用于非关键元素的文本或图标颜色
	public string? AlertColor                     { get; set; } // 提醒色，通常用于标示警告或提示信息的颜色
	public string? WarningColor                   { get; set; } // 警示色，通常用于显示需要引起注意的重要信息的颜色
	public string? DangerColor                    { get; set; } // 警告色，通常用于显示错误或危险的颜色
	public string? AccentHighlightColor           { get; set; } // 强调色区域标记颜色，通常用于高亮显示文本或元素
	public string? AccentHighlightForegroundColor { get; set; } // 强调区域标记前景色，通常用于高亮显示文本或元素

	// 构造函数，使用 TOML 表格数据初始化皮肤配置
	public SkinConfig(TomlTable value)
	{
		// 从 TOML 配置表中读取颜色值并赋值给相应属性
		BackgroundColor                = value["BackgroundColor"].ToString();
		ForegroundColor                = value["ForegroundColor"].ToString();
		AccentBackgroundColor          = value["AccentBackgroundColor"].ToString();
		AccentForegroundColor          = value["AccentForegroundColor"].ToString();
		HoverOverlayColor              = value["HoverOverlayColor"].ToString();
		PressedOverlayColor            = value["PressedOverlayColor"].ToString();
		AccentOverlayColor             = value["AccentOverlayColor"].ToString();
		SwitchOnColor                  = value["SwitchOnColor"].ToString();
		SwitchOffColor                 = value["SwitchOffColor"].ToString();
		ShadowColor                    = value["ShadowColor"].ToString();
		WeakAccentBackgroundColor      = value["WeakAccentBackgroundColor"].ToString();
		WeakAccentForegroundColor      = value["WeakAccentForegroundColor"].ToString();
		AlertColor                     = value["AlertColor"].ToString();
		WarningColor                   = value["WarningColor"].ToString();
		DangerColor                    = value["DangerColor"].ToString();
		AccentHighlightColor           = value["AccentHighlightColor"].ToString();
		AccentHighlightForegroundColor = value["AccentHighlightForegroundColor"].ToString();
	}
}
