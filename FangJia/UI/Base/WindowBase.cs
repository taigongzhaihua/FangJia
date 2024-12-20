using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
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

	private const int DWMWA_BORDER_COLOR = 34;

	private void SetWindowBorderColor(IntPtr hwnd, uint color)
	{
		DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref color, sizeof(uint));
	}

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
		/// 不使用圆角
		/// </summary>
		DWMWCP_DONOTROUND = 1,

		/// <summary>
		/// 使用圆角
		/// </summary>
		DWMWCP_ROUND = 2,

		/// <summary>
		/// 使用小圆角
		/// </summary>
		DWMWCP_ROUNDSMALL = 3
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

	[LibraryImport("dwmapi.dll", SetLastError = true)]
	private static partial void DwmSetWindowAttribute(IntPtr   hwnd,
	                                                  int      dwAttribute,
	                                                  ref uint pvAttribute,
	                                                  int      cbAttribute);

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

		StateChanged += onwindowstatechanged;
	}

	// 将 Color 转换为 ARGB 格式
	private static uint ConvertColorToArgb(Color color)
	{
		return (uint)(((0xFF - color.A) << 24) | (color.B << 16) | (color.G << 8) | color.R);
	}

	/// <summary>
	/// 设置窗口的自定义边框样式
	/// </summary>
	private void SetWindowChrome()
	{
		var windowChrome = new WindowChrome
		                   {
			                   ResizeBorderThickness = new Thickness(uniformLength: 5),  // 设置窗口的可调整边框厚度
			                   CaptionHeight         = 60,                               // 设置标题栏的高度为 0，隐藏默认标题栏
			                   CornerRadius          = new CornerRadius(8),              // 设置窗口的圆角半径
			                   GlassFrameThickness   = new Thickness(uniformLength: -1), // 设置玻璃效果的边框厚度
			                   NonClientFrameEdges = NonClientFrameEdges.Bottom | NonClientFrameEdges.Left |
			                                         NonClientFrameEdges.Right, // 设置非客户区边缘
			                   UseAeroCaptionButtons = false,                   // 禁用默认的标题栏按钮
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
		var hWnd = new WindowInteropHelper(window: this).Handle; // 获取窗口的句柄
		var color     = (Color)FindResource("AccentColor");
		var argbColor = ConvertColorToArgb(color);
		SetWindowBorderColor(hWnd, argbColor);
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

	private void onwindowstatechanged(object? sender, EventArgs e)
	{
		if (WindowState == WindowState.Maximized)
		{
			var screen = SystemParameters.WorkArea; // 系统工作区域
			MaxWidth        = screen.Width  + 16;
			MaxHeight       = screen.Height + 16;
			BorderThickness = new Thickness(8);
		}
		else
		{
			BorderThickness = new Thickness(0);
		}
	}
}
