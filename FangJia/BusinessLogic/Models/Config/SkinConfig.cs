// ReSharper disable UnusedAutoPropertyAccessor.Global

using Tomlyn.Model;

namespace FangJia.BusinessLogic.Models.Config;

public class SkinConfig(TomlTable value)
{
    public string? WindowBg                        { get; set; } = value["WindowBg"].ToString();                        // 窗口背景颜色
    public string? LayoutBg                        { get; set; } = value["LayoutBg"].ToString();                        // 布局背景颜色
    public string? ControlBg                       { get; set; } = value["ControlBg"].ToString();                       // 控件背景颜色
    public string? PrimaryTitle                    { get; set; } = value["PrimaryTitle"].ToString();                    // 一级标题颜色
    public string? SecondaryTitle                  { get; set; } = value["SecondaryTitle"].ToString();                  // 二级标题颜色
    public string? TertiaryTitle                   { get; set; } = value["TertiaryTitle"].ToString();                   // 三级标题颜色
    public string? BodyText                        { get; set; } = value["BodyText"].ToString();                        // 正文标题颜色
    public string? ListItemBg                      { get; set; } = value["ListItemBg"].ToString();                      // 列表项背景颜色
    public string? ListItemFg                      { get; set; } = value["ListItemFg"].ToString();                      // 列表项前景颜色
    public string? ListItemSelectedBg              { get; set; } = value["ListItemSelectedBg"].ToString();              // 列表项选中背景颜色
    public string? ListItemSelectedFg              { get; set; } = value["ListItemSelectedFg"].ToString();              // 列表项选中前景颜色
    public string? ListItemHoverBg                 { get; set; } = value["ListItemHoverBg"].ToString();                 // 列表项悬停背景颜色
    public string? ListItemHoverFg                 { get; set; } = value["ListItemHoverFg"].ToString();                 // 列表项悬停前景颜色
    public string? ButtonBg                        { get; set; } = value["ButtonBg"].ToString();                        // 按钮背景颜色
    public string? ButtonFg                        { get; set; } = value["ButtonFg"].ToString();                        // 按钮前景颜色
    public string? ButtonDisabledBg                { get; set; } = value["ButtonDisabledBg"].ToString();                // 按钮禁用背景颜色
    public string? ButtonDisabledFg                { get; set; } = value["ButtonDisabledFg"].ToString();                // 按钮禁用前景颜色
    public string? TransparentButtonFg             { get; set; } = value["TransparentButtonFg"].ToString();             // 透明按钮前景颜色
    public string? TransparentButtonBorder         { get; set; } = value["TransparentButtonBorder"].ToString();         // 透明按钮边框颜色
    public string? TransparentButtonDisabledFg     { get; set; } = value["TransparentButtonDisabledFg"].ToString();     // 透明按钮禁用前景颜色
    public string? TransparentButtonDisabledBorder { get; set; } = value["TransparentButtonDisabledBorder"].ToString(); // 透明按钮禁用边框颜色
    public string? ButtonHoverMask                 { get; set; } = value["ButtonHoverMask"].ToString();                 // 按钮悬停叠加颜色
    public string? ButtonPressedMask               { get; set; } = value["ButtonPressedMask"].ToString();               // 按钮按下叠加颜色
    public string? AccentColor                     { get; set; } = value["AccentColor"].ToString();                     // 强调背景颜色
    public string? AccentFg                        { get; set; } = value["AccentFg"].ToString();                        // 强调前景颜色
    public string? AccentTitle                     { get; set; } = value["AccentTitle"].ToString();                     // 强调标题颜色
    public string? AccentHoverBg                   { get; set; } = value["AccentHoverBg"].ToString();                   // 强调悬停背景颜色
    public string? AccentHoverFg                   { get; set; } = value["AccentHoverFg"].ToString();                   // 强调悬停前景颜色
    public string? AccentPressedBg                 { get; set; } = value["AccentPressedBg"].ToString();                 // 强调按下背景颜色
    public string? AccentPressedFg                 { get; set; } = value["AccentPressedFg"].ToString();                 // 强调按下前景颜色
    public string? CheckboxUncheckedIcon           { get; set; } = value["CheckboxUncheckedIcon"].ToString();           // 复选框未选中图标颜色
    public string? CheckboxHoverIcon               { get; set; } = value["CheckboxHoverIcon"].ToString();               // 复选框悬停图标颜色
    public string? CheckboxCheckedIcon             { get; set; } = value["CheckboxCheckedIcon"].ToString();             // 复选框选中图标颜色
    public string? CheckboxText                    { get; set; } = value["CheckboxText"].ToString();                    // 复选框文字颜色
    public string? CheckboxHoverText               { get; set; } = value["CheckboxHoverText"].ToString();               // 复选框悬停文字颜色
    public string? CheckboxCheckedText             { get; set; } = value["CheckboxCheckedText"].ToString();             // 复选框选中文字颜色
    public string? ToggleButtonBg                  { get; set; } = value["ToggleButtonBg"].ToString();                  // 切换按钮背景颜色
    public string? ToggleButtonFg                  { get; set; } = value["ToggleButtonFg"].ToString();                  // 切换按钮前景颜色
    public string? ToggleButtonHoverBg             { get; set; } = value["ToggleButtonHoverBg"].ToString();             // 切换按钮悬停背景颜色
    public string? ToggleButtonHoverFg             { get; set; } = value["ToggleButtonHoverFg"].ToString();             // 切换按钮悬停前景颜色
    public string? ToggleButtonSelectedBg          { get; set; } = value["ToggleButtonSelectedBg"].ToString();          // 切换按钮选中背景颜色
    public string? ToggleButtonSelectedFg          { get; set; } = value["ToggleButtonSelectedFg"].ToString();          // 切换按钮选中前景颜色
    public string? SwitchOffBg                     { get; set; } = value["SwitchOffBg"].ToString();                     // 开关关闭背景颜色
    public string? SwitchHoverBg                   { get; set; } = value["SwitchHoverBg"].ToString();                   // 开关悬停背景颜色
    public string? SwitchOnBg                      { get; set; } = value["SwitchOnBg"].ToString();                      // 开关开启背景颜色
    public string? SwitchOffFg                     { get; set; } = value["SwitchOffFg"].ToString();                     // 开关关闭前景颜色
    public string? SwitchHoverFg                   { get; set; } = value["SwitchHoverFg"].ToString();                   // 开关悬停前景颜色
    public string? SwitchOnFg                      { get; set; } = value["SwitchOnFg"].ToString();                      // 开关开启前景颜色
    public string? ShadowColor                     { get; set; } = value["ShadowColor"].ToString();                     // 阴影颜色
    public string? SuccessColor                    { get; set; } = value["SuccessColor"].ToString();                    // 成功颜色
    public string? InfoColor                       { get; set; } = value["InfoColor"].ToString();                       // 信息颜色
    public string? WarningColor                    { get; set; } = value["WarningColor"].ToString();                    // 警告颜色
    public string? ErrorColor                      { get; set; } = value["ErrorColor"].ToString();                      // 错误颜色

    // 构造函数，使用 TOML 表格数据初始化皮肤配置
    // 构造函数，使用 TOML 表格数据初始化皮肤配置
}

