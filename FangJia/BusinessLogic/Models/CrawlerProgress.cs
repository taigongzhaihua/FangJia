using System.Diagnostics.CodeAnalysis;

namespace FangJia.BusinessLogic.Models;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
public struct CrawlerProgress(int totalLength, int currentProgress, bool isRunning) : IEquatable<CrawlerProgress>
{
	public int          TotalLength     { get; set; } = totalLength;     // 总长度
	public int          CurrentProgress { get; set; } = currentProgress; // 当前进度
	public List<string> LogList         { get; }      = [];              // 日志列表
	public string       LatestLog       { get; set; } = string.Empty;    // 最新日志
	public bool         IsRunning       { get; set; } = isRunning;       // 是否运行中

	// 构造函数

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

	// 重写 Equals 方法
	public override bool Equals(object? obj)
	{
		if (obj is CrawlerProgress other)
		{
			return TotalLength     == other.TotalLength     &&
			       CurrentProgress == other.CurrentProgress &&
			       IsRunning       == other.IsRunning       &&
			       LatestLog       == other.LatestLog       &&
			       LogList.SequenceEqual(other.LogList);
		}

		return false;
	}

	// 重写 GetHashCode 方法
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		hashCode.Add(TotalLength);
		hashCode.Add(CurrentProgress);
		hashCode.Add(IsRunning);
		hashCode.Add(LatestLog);
		hashCode.Add(LogList);


		return hashCode.ToHashCode();
	}

	public static bool operator ==(CrawlerProgress left, CrawlerProgress right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(CrawlerProgress left, CrawlerProgress right)
	{
		return !(left == right);
	}

	public bool Equals(CrawlerProgress other)
	{
		return TotalLength     == other.TotalLength     &&
		       CurrentProgress == other.CurrentProgress &&
		       IsRunning       == other.IsRunning       &&
		       LatestLog       == other.LatestLog       &&
		       LogList.Equals(other.LogList);
	}
}
