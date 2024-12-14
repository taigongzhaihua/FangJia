using NLog;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Windows;

namespace FangJia.BusinessLogic.Services;

/// <summary>
/// 管道服务，用于实现单实例通信机制。
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static partial class PipeService
{
	private static readonly Logger                   Logger   = LogManager.GetCurrentClassLogger();
	private const           string                   PipeName = "FangJia";
	private static          CancellationTokenSource? _cancellationTokenSource;

	/// <summary>
	/// 通知已存在的实例。
	/// 如果管道服务器已存在，则尝试连接，并发送消息通知它显示主窗口。
	/// </summary>
	/// <remarks>
	/// 实现步骤：
	/// 1. 初始化连接状态为 false，并设置最大重试次数为 3。
	/// 2. 进入循环，尝试连接到已存在的管道服务器。
	/// 3. 如果连接成功，则向服务器发送 "SHOW" 消息，并设置连接状态为 true。
	/// 4. 如果连接失败，则记录日志并重试，直到达到最大重试次数。
	/// 5. 如果所有重试均失败，则记录错误日志并退出。
	/// </remarks>
	public static void NotifyExistingInstance()
	{
		var connected = false;
		var retries   = 3; // 最大重试次数

		while (!connected && retries > 0)
		{
			try
			{
				Logger.Info("尝试连接已有实例...");
				using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
				client.Connect(1000); // 尝试连接到已有实例，超时时间为1000毫秒

				using var writer = new StreamWriter(client);
				writer.WriteLine("SHOW"); // 向服务器发送指令
				writer.Flush();

				connected = true;
				Logger.Info("成功连接到已有实例，并发送消息。");
			}
			catch (Exception ex)
			{
				retries--;
				Logger.Warn($"连接失败，剩余重试次数: {retries}");
				Logger.Error($"连接已有实例时发生异常：{ex.Message}", ex);

				if (retries > 0)
					Thread.Sleep(500); // 短暂等待后重试
			}
		}

		if (!connected)
		{
			Logger.Error("多次尝试后仍无法连接到已有实例，操作失败。");
		}
	}

	/// <summary>
	/// 当以重启状态启动应用程序时，通知已存在的实例关闭，保留新实例。
	/// </summary>
	/// <remarks>
	/// 实现步骤：
	/// 1. 初始化连接状态为 false，并设置最大重试次数为 3。
	/// 2. 进入循环，尝试连接到已存在的管道服务器。
	/// 3. 如果连接成功，则向服务器发送 "RESTART" 消息，并设置连接状态为 true。
	/// 4. 如果连接失败，则记录日志并重试，直到达到最大重试次数。
	/// 5. 如果所有重试均失败，则记录错误日志并退出。
	/// </remarks>
	public static void OnAppRestarted()
	{
		var connected = false;
		var retries   = 3; // 最大重试次数

		while (!connected && retries > 0)
		{
			try
			{
				Logger.Info("尝试连接已有实例...");
				using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
				client.Connect(1000); // 尝试连接到已有实例，超时时间为1000毫秒

				using var writer = new StreamWriter(client);
				writer.WriteLine("RESTART"); // 向服务器发送指令
				writer.Flush();

				connected = true;
				Logger.Info("成功连接到已有实例，并发送RESTART消息。");
			}
			catch (Exception ex)
			{
				retries--;
				Logger.Warn($"连接失败，剩余重试次数: {retries}");
				Logger.Error($"连接已有实例时发生异常：{ex.Message}", ex);

				if (retries > 0)
					Thread.Sleep(500); // 短暂等待后重试
			}
		}

		if (!connected)
		{
			Logger.Error("多次尝试后仍无法连接到已有实例，操作失败。");
		}
	}

	/// <summary>
	/// 重启应用程序。
	/// </summary>
	/// <remarks>
	/// 实现步骤：
	/// 1. 获取当前应用程序的路径。
	/// 2. 使用 Process.Start 启动新的应用程序实例，并传递 "ReStart" 参数。
	/// 3. 调用 Application.Current.Shutdown 关闭当前实例。
	/// </remarks>
	public static void RestartApp()
	{
		Logger.Info("准备重启应用程序...");
		var appPath = Process.GetCurrentProcess().MainModule?.FileName;
		Process.Start(new ProcessStartInfo
		              {
			              FileName        = appPath,
			              Arguments       = "ReStart",
			              UseShellExecute = true
		              });
		Application.Current.Shutdown();
	}

	/// <summary>
	/// 启动管道服务器。
	/// 持续监听管道连接，接收其他实例的消息并进行处理。
	/// </summary>
	/// <remarks>
	/// 实现步骤：
	/// 1. 初始化 CancellationTokenSource 用于控制服务器停止。
	/// 2. 进入循环，持续监听管道连接。
	/// 3. 当客户端连接时，读取客户端发送的消息。
	/// 4. 根据消息内容执行相应的操作：
	///    - 如果是 "SHOW" 消息，则显示主窗口。
	///    - 如果是 "RESTART" 消息，则关闭当前实例。
	/// 5. 断开当前连接，准备接收下一个连接。
	/// 6. 如果接收到取消令牌信号，则退出循环并停止服务器。
	/// </remarks>
	public static void StartPipeServer()
	{
		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;

		Logger.Info("管道服务器已启动，等待连接...");
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				// 创建命名管道服务端
				using var server = new NamedPipeServerStream(
				                                             PipeName,
				                                             PipeDirection.In,
				                                             NamedPipeServerStream.MaxAllowedServerInstances,
				                                             PipeTransmissionMode.Message);

				server.WaitForConnection(); // 等待客户端连接
				Logger.Info("客户端已连接到管道。");

				// 读取客户端发送的消息
				using var reader  = new StreamReader(server);
				var       message = reader.ReadLine();

				switch (message)
				{
					case "SHOW":
						Logger.Info("接收到 SHOW 消息，准备显示主窗口。");
						ShowMainWindow();
						break;
					case "RESTART":
						Logger.Info("接收到 RESTART 消息，关闭当前实例，以应用新实例。");
						Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
						break;
				}

				server.Disconnect(); // 断开当前连接，准备接收下一个连接
				Logger.Info("管道连接已断开。");
			}
			catch (ObjectDisposedException ex)
			{
				Logger.Warn($"管道服务器已关闭：{ex.Message}");
				break; // 管道已关闭，退出循环
			}
			catch (Exception ex)
			{
				Logger.Error($"管道服务器发生异常：{ex.Message}", ex);
			}
		}

		Logger.Info("管道服务器已停止运行。");
	}

	/// <summary>
	/// 停止管道服务器。
	/// 通过取消令牌通知服务器退出。
	/// </summary>
	/// <remarks>
	/// 实现步骤：
	/// 1. 调用 CancellationTokenSource.Cancel 方法通知服务器停止。
	/// 2. 记录日志表示服务器已成功停止。
	/// 3. 捕获并记录任何异常。
	/// </remarks>
	public static void StopPipeServer()
	{
		try
		{
			Logger.Info("正在停止管道服务器...");
			_cancellationTokenSource?.Cancel();
			Logger.Info("管道服务器已成功停止。");
		}
		catch (Exception ex)
		{
			Logger.Error($"停止管道服务器时发生异常：{ex.Message}", ex);
		}
	}

	/// <summary>
	/// 显示主窗口。
	/// 当接收到 "SHOW" 消息时调用，用于激活和显示主窗口。
	/// </summary>
	/// <remarks>
	/// 实现步骤：
	/// 1. 使用 Dispatcher.Invoke 确保在 UI 线程上执行操作。
	/// 2. 获取当前应用程序的主窗口。
	/// 3. 如果主窗口未初始化，则记录警告并返回。
	/// 4. 显示主窗口，恢复窗口状态为正常，并激活窗口。
	/// 5. 使用 SetForegroundWindow 将窗口置于最前。
	/// </remarks>
	private static void ShowMainWindow()
	{
		Application.Current.Dispatcher.Invoke(() =>
		                                      {
			                                      var mainWindow = Application.Current.MainWindow;
			                                      if (mainWindow == null)
			                                      {
				                                      Logger.Warn("主窗口未初始化，无法显示。");
				                                      return;
			                                      }

			                                      mainWindow.Show();                           // 显示窗口
			                                      mainWindow.WindowState = WindowState.Normal; // 恢复窗口状态
			                                      mainWindow.Activate();                       // 激活窗口
			                                      mainWindow.Focus();

			                                      // 将窗口置于最前
			                                      var windowInteropHelper =
				                                      new System.Windows.Interop.WindowInteropHelper(mainWindow);
			                                      windowInteropHelper.EnsureHandle();
			                                      var hwnd = windowInteropHelper.Handle;
			                                      SetForegroundWindow(hwnd);
		                                      });
	}

	/// <summary>
	/// 将窗口置于最前。
	/// </summary>
	/// <param name="hWnd">窗口句柄。</param>
	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial void SetForegroundWindow(IntPtr hWnd);
}
