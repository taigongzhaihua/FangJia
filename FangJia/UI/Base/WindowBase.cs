using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace FangJia.UI.Base;

/// <summary>
/// BaseWindow 类作为整个应用所有自定义窗口的基类，继承自 Window 类。
/// 在保留系统边框的前提下，只实现窗口拖动一个功能，并将窗口圆角设定为标准圆角
/// </summary>
[SuppressMessage(category: "ReSharper", checkId: "StringLiteralTypo")]
[SuppressMessage(category: "ReSharper", checkId: "InconsistentNaming")]
[SuppressMessage(category: "ReSharper", checkId: "IdentifierTypo")]
[SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Local")]
public partial class WindowBase : Window
{
	/// <summary>
	/// DWM 属性常量，用于设置窗口圆角的属性标识符
	/// </summary>
	private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

	/// <summary>
	/// 枚举表示不同的窗口圆角偏好设置
	/// </summary>
	private enum DwmWindowCornerPreference
	{
		/// <summary>
		/// 默认圆角设置，取决于系统设置
		/// </summary>
		DWMWCP_DEFAULT = 0,

		/// <summary>
		/// 使用圆角
		/// </summary>
		DWMWCP_ROUND = 2,

		/// <summary>
		/// 使用小圆角
		/// </summary>
		DWMWCP_ROUNDED = 3
	}

	/// <summary>
	/// 导入 DWM API，用于设置窗口的属性
	/// </summary>
	/// <param name="hwnd">窗口的句柄</param>
	/// <param name="attr">属性标识符</param>
	/// <param name="attrValue">属性值</param>
	/// <param name="attrSize">属性大小</param>
	[LibraryImport(libraryName: "dwmapi.dll", SetLastError = true)]
	private static partial void DwmSetWindowAttribute(IntPtr                        hwnd,
	                                                  int                           attr,
	                                                  ref DwmWindowCornerPreference attrValue,
	                                                  int                           attrSize);

	/// <summary>
	/// BaseWindow 构造函数，初始化窗口组件并设置自定义样式
	/// </summary>
	public WindowBase()
	{
		SetWindowChrome(); // 设置窗口的自定义边框样式

		// 窗口初始化完成后设置事件处理，设置窗口的圆角
		SourceInitialized += OnSourceInitialized;

		// 允许用户通过拖动窗口空白区域来移动窗口
		MouseLeftButtonDown += (_, _) => DragMove();
	}

	/// <summary>
	/// 设置窗口的自定义边框样式
	/// </summary>
	private void SetWindowChrome()
	{
		var windowChrome = new WindowChrome
		                   {
			                   ResizeBorderThickness = new Thickness(uniformLength: 5), // 设置窗口的可调整边框厚度
			                   CaptionHeight         = 0,                               // 设置标题栏的高度为 0，隐藏默认标题栏
			                   CornerRadius          = new CornerRadius(15)             // 设置窗口的圆角半径
		                   };
		WindowChrome.SetWindowChrome(window: this, chrome: windowChrome); // 应用自定义的 WindowChrome 到窗口
	}

	/// <summary>
	/// 在窗口源初始化完成时设置圆角属性
	/// </summary>
	/// <param name="sender">事件的发送者</param>
	/// <param name="e">事件参数</param>
	private void OnSourceInitialized(object? sender, EventArgs e)
	{
		SetWindowCornerRadiusIfApplicable(); // 如果适用，设置窗口圆角
	}

	/// <summary>
	/// 设置窗口的圆角属性（仅适用于 Windows 11 或更高版本）
	/// </summary>
	private void SetWindowCornerRadiusIfApplicable()
	{
		if (!IsWindows11OrGreater) return; // 如果不是 Windows 11 或更高版本，则不进行任何操作

		var hWnd       = new WindowInteropHelper(window: this).Handle; // 获取窗口的句柄
		var preference = DwmWindowCornerPreference.DWMWCP_ROUND;       // 设置为圆角样式

		// 调用 DWM API 设置窗口的圆角属性
		DwmSetWindowAttribute(hwnd: hWnd,
		                      attr: DWMWA_WINDOW_CORNER_PREFERENCE,
		                      attrValue: ref preference,
		                      attrSize: sizeof(uint));
	}

	/// <summary>
	/// 静态属性用于判断当前操作系统是否为 Windows 11 或更高版本
	/// </summary>
	private static bool IsWindows11OrGreater =>
		Environment.OSVersion.Version >=
		new Version(major: 10, minor: 0, build: 22000, revision: 0); // Windows 11 的版本号为 10.0.22000
}
