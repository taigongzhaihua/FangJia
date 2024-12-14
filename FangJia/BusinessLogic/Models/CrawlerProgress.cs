namespace FangJia.BusinessLogic.Models;

public struct CrawlerProgress
{
	public int          TotalLength     { get; set; } // 总长度
	public int          CurrentProgress { get; set; } // 当前进度
	public List<string> LogList         { get; }      // 日志列表
	public string       LatestLog       { get; set; } // 最新日志
	public bool         IsRunning       { get; set; } // 是否运行中

	// 构造函数
	public CrawlerProgress(int totalLength, int currentProgress, bool isRunning)
	{
		TotalLength     = totalLength;
		CurrentProgress = currentProgress;
		LogList         = [];
		LatestLog       = string.Empty;
		IsRunning       = isRunning;
	}

	// 更新最新日志
	public CrawlerProgress AddLog(string log)
	{
		LogList.Add(log);
		LatestLog = log;
		return this;
	}

	// 更新进度
	public CrawlerProgress UpdateProgress(int newProgress)
	{
		if (newProgress > TotalLength)
		{
			throw new ArgumentOutOfRangeException(nameof(newProgress), @"进度不能超过总长度。");
		}

		CurrentProgress = newProgress;
		return this;
	}

	// 重置进度
	public CrawlerProgress Reset()
	{
		CurrentProgress = 0;
		TotalLength     = 0;
		LogList.Clear();
		LatestLog = string.Empty;
		IsRunning = false;
		return this;
	}
}
