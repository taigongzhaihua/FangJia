// 皮肤配置类
using Tomlyn.Model;

namespace FangJia.Models.ConfigModels;

public class SkinConfig
{
    public string? BackgroundColor { get; set; }
    public string? ForegroundColor { get; set; }
    public string? AccentBackgroundColor { get; set; }
    public string? AccentForegroundColor { get; set; }
    public string? HoverOverlayColor { get; set; }
    public string? PressedOverlayColor { get; set; }
    public string? AccentOverlayColor { get; set; }
    public string? SwitchOnColor { get; set; }
    public string? SwitchOffColor { get; set; }
    public string? ShadowColor { get; set; }

    public SkinConfig()
    {
    }
    public SkinConfig(TomlTable v)
    {
        BackgroundColor = v["BackgroundColor"].ToString();
        ForegroundColor = v["ForegroundColor"].ToString();
        AccentBackgroundColor = v["AccentBackgroundColor"].ToString();
        AccentForegroundColor = v["AccentForegroundColor"].ToString();
        HoverOverlayColor = v["HoverOverlayColor"].ToString();
        PressedOverlayColor = v["PressedOverlayColor"].ToString();
        AccentOverlayColor = v["AccentOverlayColor"].ToString();
        SwitchOnColor = v["SwitchOnColor"].ToString();
        SwitchOffColor = v["SwitchOffColor"].ToString();
        ShadowColor = v["ShadowColor"].ToString();
    }
}