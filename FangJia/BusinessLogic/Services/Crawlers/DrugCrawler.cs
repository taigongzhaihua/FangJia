using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Data;
using HtmlAgilityPack;
using NLog;
using System.Net.Http;
using System.Text;
using FangJia.BusinessLogic.Models;

namespace FangJia.BusinessLogic.Services.Crawlers;

/// <summary>
/// 药品爬虫服务类，实现 ICrawler&lt;Drug&gt; 接口，用于从指定网站爬取药品信息。
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class DrugCrawler : ICrawler<Drug>
{
	// 
	private static readonly HttpClient                 HttpClient = new();
	private static readonly Logger                     Logger     = LogManager.GetCurrentClassLogger();
	private const           string                     BaseUrl    = "https://www.zhongyifangji.com";
	private const           string                     PageUrl    = "https://www.zhongyifangji.com/materials/index/p/";
	private                 IProgress<CrawlerProgress> _progress  = null!;
	private                 CrawlerProgress            _progressReport;

	/// <summary>
	/// 获取药品列表的异步方法。
	/// 该方法通过爬取指定网站的多个页面，获取药品的详细信息，并返回一个包含所有药品的列表。
	/// </summary>
	/// <returns>包含所有药品的列表</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 初始化一个空的药品列表。
	/// 2. 从第1页到第21页，依次获取每页的URL。
	/// 3. 对于每个页面，调用 <see cref="GetLinksAsync(string)"/> 方法获取该页面上所有药品的链接。
	/// 4. 对于每个药品链接，调用 <see cref="GetDrugDetailsAsync(string)"/> 方法获取药品的详细信息。
	/// 5. 如果药品详细信息获取成功，将其添加到药品列表中。
	/// 6. 爬取完成后，记录总药品数量并返回药品列表。
	/// </remarks>
	public async Task<List<Drug>> GetListAsync(IProgress<CrawlerProgress> progress)
	{
		_progress       = progress;
		_progressReport = new CrawlerProgress(21, 0, true);
		_progress.Report(_progressReport);

		// 初始化一个空的药品列表
		ConcurrentBag<Drug> drugs = new();

		try
		{
			Logger.Info("开始爬取药品数据...");

			// 从第1页到第21页，依次获取每页的URL
			var pageUrls = Enumerable.Range(1, 21).Select(i => $"{PageUrl}{i}").ToList();

			// 并发获取每页的所有药品链接
			var linkTasks = pageUrls.Select(async url =>
			                                {
				                                var pageNumber = url.Split('/').Last();
				                                var log        = $"正在获取第 {pageNumber} 页: {url}";
				                                Logger.Info(log);
				                                _progress.Report(_progressReport.AddLog(log));

				                                // 调用 GetLinksAsync 方法获取该页面上所有药品的链接
				                                return await GetLinksAsync(url);
			                                }).ToList();

			// 等待所有链接获取任务完成
			var linkResults = await Task.WhenAll(linkTasks);

			// 将所有链接合并到一个列表中
			var links = linkResults.SelectMany(l => l).ToList();
			_progressReport.TotalLength = links.Count + 21;
			_progressReport.UpdateProgress(21);
			_progress.Report(_progressReport);

			// 使用 SemaphoreSlim 限制并发线程数为20
			using var semaphore = new SemaphoreSlim(20);

			// 并发获取每个药品的详细信息
			var drugTasks =
				links.Select(async link =>
				             {
					             await semaphore.WaitAsync(); // 等待获取信号量
					             try
					             {
						             var log = $"正在获取药品详细信息: {link}";
						             Logger.Info(log);
						             _progressReport.AddLog(log);
						             _progress.Report(_progressReport);

						             var drug = await GetDrugDetailsAsync(link);

						             // 如果药品详细信息获取成功，将其添加到药品列表中
						             if (drug != null)
						             {
							             drugs.Add(drug);
						             }

						             _progress.Report
							             (_progressReport
							              .UpdateProgress(_progressReport.CurrentProgress + 1)
							              .AddLog($"获取 {drug?.Name ?? "未知"} 信息完毕"));
					             }
					             finally
					             {
						             semaphore.Release(); // 释放信号量
					             }
				             }).ToList();

			// 等待所有药品详细信息获取任务完成
			await Task.WhenAll(drugTasks);

			// 爬取完成后，记录总药品数量
			Logger.Info($"爬取完成。共获取药品数量: {drugs.Count}");
			_progressReport.AddLog($"爬取完成。共获取药品数量: {drugs.Count}");
			_progressReport.IsRunning = false;
			_progress.Report(_progressReport);
		}
		catch (Exception ex)
		{
			// 记录爬取过程中的错误
			Logger.Error($"爬取过程中发生错误: {ex.Message}");
		}

		await Task.Delay(500);
		// 返回药品列表
		return drugs.ToList();
	}

	/// <summary>
	/// 获取指定页面中所有药品链接的异步方法。
	/// 该方法通过解析指定页面的HTML内容，提取出所有药品的详细信息链接，并返回一个包含这些链接的列表。
	/// </summary>
	/// <param name="pageUrl">要爬取的页面URL</param>
	/// <returns>包含所有药品详细信息链接的列表</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 初始化一个空的链接列表。
	/// 2. 使用 HttpClient 获取指定页面的HTML内容。
	/// 3. 使用 HtmlAgilityPack 解析HTML内容。
	/// 4. 通过XPath选择器提取出所有包含药品链接的节点。
	/// 5. 对于每个节点，获取其 href 属性值，并拼接成完整的URL。
	/// 6. 将所有提取到的链接添加到链接列表中。
	/// 7. 返回链接列表。
	/// 8. 如果在过程中发生异常，记录错误信息并返回空的链接列表。
	/// </remarks>
	private async Task<List<string>> GetLinksAsync(string pageUrl)
	{
		// 初始化一个空的链接列表
		List<string> links = [];
		try
		{
			// 使用 HttpClient 获取指定页面的HTML内容
			var html     = await HttpClient.GetStringAsync(pageUrl);
			var document = new HtmlDocument();
			document.LoadHtml(html);

			// 使用 HtmlAgilityPack 解析HTML内容，通过XPath选择器提取出所有包含药品链接的节点
			var nodes =
				document.DocumentNode
				        .SelectNodes("//div[contains(@class, 'card shadow-sm border-0')]/a");
			if (nodes != null)
			{
				// 对于每个节点，获取其 href 属性值，并拼接成完整的URL
				links = nodes.Select
					(node => new StringBuilder().Append(BaseUrl)
					                            .Append(node.GetAttributeValue("href", ""))
					                            .ToString()
					).ToList();
				_progressReport.AddLog($@"在页面 {pageUrl} 上找到 {links.Count} 个链接");
				_progress.Report(_progressReport);
			}
		}
		catch (Exception ex)
		{
			// 如果在过程中发生异常，记录错误信息
			Logger.Error($@"从 {pageUrl} 获取链接时发生错误: {ex.Message}");
		}

		// 返回链接列表
		return links;
	}

	/// <summary>
	/// 获取药品详细信息的异步方法。
	/// 该方法通过解析指定药品页面的HTML内容，提取出药品的各项详细信息，并返回一个包含这些信息的 <see cref="Drug"/> 对象。
	/// </summary>
	/// <param name="url">药品详细信息的页面URL</param>
	/// <returns>包含药品详细信息的 <see cref="Drug"/> 对象，如果获取失败则返回 null</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 使用 HttpClient 获取指定药品页面的HTML内容。
	/// 2. 使用 HtmlAgilityPack 解析HTML内容。
	/// 3. 初始化一个新的 <see cref="Drug"/> 对象。
	/// 4. 通过XPath选择器提取药品的各项详细信息，并将其赋值给 <see cref="Drug"/> 对象的相应属性。
	/// 5. 调用 <see cref="GetDrugImageAsync(HtmlDocument)"/> 方法获取药品的图片信息，并将其赋值给 <see cref="Drug"/> 对象的 <see cref="Drug.DrugImage"/> 属性。
	/// 6. 返回包含药品详细信息的 <see cref="Drug"/> 对象。
	/// 7. 如果在过程中发生异常，记录错误信息并返回 null。
	/// </remarks>
	private async Task<Drug?> GetDrugDetailsAsync(string url)
	{
		try
		{
			// 使用 HttpClient 获取指定药品页面的HTML内容
			var html     = await HttpClient.GetStringAsync(url);
			var document = new HtmlDocument();
			document.LoadHtml(html);

			// 初始化一个新的 Drug 对象
			var drug = new Drug
			           {
				           // 通过XPath选择器提取药品的各项详细信息，并将其赋值给 Drug 对象的相应属性
				           Name        = GetInnerText(document, "//strong[text()='名称']/following-sibling::div"),
				           EnglishName = GetInnerText(document, "//strong[text()='英文名']/following-sibling::div"),
				           LatinName   = GetInnerText(document, "//strong[text()='拉丁名']/following-sibling::div"),
				           Category    = GetInnerText(document, "//strong[text()='分类']/following-sibling::div"),
				           Origin      = GetInnerText(document, "//strong[text()='产地']/following-sibling::div"),
				           Properties  = GetInnerText(document, "//strong[text()='性状']/following-sibling::div"),
				           Quality     = GetInnerText(document, "//strong[text()='品质']/following-sibling::div"),
				           Taste       = GetInnerText(document, "//strong[text()='性味']/following-sibling::div"),
				           Effect      = GetInnerText(document, "//strong[text()='功效']/following-sibling::div"),
				           Notes       = GetInnerText(document, "//strong[text()='注解']/following-sibling::div"),
				           Processed   = GetInnerText(document, "//strong[text()='炙品']/following-sibling::div"),
				           Source      = GetInnerText(document, "//strong[text()='来源']/following-sibling::div"),
				           // 调用 GetDrugImageAsync 方法获取药品的图片信息，并将其赋值给 Drug 对象的 DrugImage 属性
				           DrugImage = await GetDrugImageAsync(document)
			           };

			// 返回包含药品详细信息的 Drug 对象
			return drug;
		}
		catch (Exception ex)
		{
			// 如果在过程中发生异常，记录错误信息并返回 null
			Logger.Error($"Error fetching drug details from {url}: {ex.Message}");
			return null;
		}
	}

	/// <summary>
	/// 获取药品图片信息的异步方法。
	/// 该方法通过解析指定药品页面的HTML内容，提取出药品图片的URL，并下载图片，返回一个包含图片信息的 <see cref="DrugImage"/> 对象。
	/// </summary>
	/// <param name="document">包含药品页面HTML内容的 <see cref="HtmlDocument"/> 对象</param>
	/// <returns>包含药品图片信息的 <see cref="DrugImage"/> 对象</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 初始化一个新的 <see cref="DrugImage"/> 对象。
	/// 2. 使用 HtmlAgilityPack 解析HTML内容，通过XPath选择器提取出包含药品图片的节点。
	/// 3. 如果找到图片节点，获取其 src 属性值，并拼接成完整的URL。
	/// 4. 使用 HttpClient 下载图片内容，并将其赋值给 <see cref="DrugImage"/> 对象的 <see cref="DrugImage.Image"/> 属性。
	/// 5. 返回包含图片信息的 <see cref="DrugImage"/> 对象。
	/// 6. 如果在过程中发生异常，记录错误信息并返回空的 <see cref="DrugImage"/> 对象。
	/// </remarks>
	private async Task<DrugImage> GetDrugImageAsync(HtmlDocument document)
	{
		// 初始化一个新的 DrugImage 对象
		var drugImage = new DrugImage();

		try
		{
			// 使用 HtmlAgilityPack 解析HTML内容，通过XPath选择器提取出包含药品图片的节点
			var imgNode =
				document.DocumentNode.SelectSingleNode("//div[contains(@class, 'col-6 d-none d-lg-block')]/img");
			Logger.Debug($"找到图片节点: {imgNode}");

			if (imgNode != null)
			{
				// 获取图片节点的 src 属性值，并拼接成完整的URL
				var imgUrl = imgNode.GetAttributeValue("src", "");
//				Logger.Debug($"图片URL = {imgUrl}");

				if (!string.IsNullOrWhiteSpace(imgUrl))
				{
					// 确保图片URL是完整的，如果不是，则拼接 BaseUrl
					imgUrl = imgUrl.StartsWith("http") ? imgUrl : BaseUrl + imgUrl;

					// 使用 HttpClient 下载图片内容
					Logger.Info($"正在获取图片: {imgUrl}");
					_progress.Report(_progressReport.AddLog($"正在获取图片: {imgUrl}"));
					var imgBytes = await HttpClient.GetByteArrayAsync(imgUrl);
					drugImage.Image = imgBytes;
				}
			}
			else
			{
				// 如果未找到图片节点，记录信息
				Logger.Info("未找到图片节点: //div[contains(@class, 'col-6 d-none d-lg-block')]/img");
			}
		}
		catch (Exception ex)
		{
			// 如果在过程中发生异常，记录错误信息
			Logger.Error($"获取图片时发生错误: {ex.Message}");
		}

		// 返回包含图片信息的 DrugImage 对象
		return drugImage;
	}

	/// <summary>
	/// 获取指定XPath节点的内部文本内容。
	/// 该方法通过解析指定药品页面的HTML内容，提取出指定XPath节点的内部文本，并返回该文本内容。
	/// </summary>
	/// <param name="document">包含药品页面HTML内容的 <see cref="HtmlDocument"/> 对象</param>
	/// <param name="xpath">用于提取节点的XPath表达式</param>
	/// <returns>指定节点的内部文本内容，如果未找到节点则返回 null</returns>
	/// <remarks>
	/// 实现步骤：
	/// 1. 使用 HtmlAgilityPack 解析HTML内容，通过XPath选择器提取出指定节点。
	/// 2. 如果找到节点，返回节点的内部文本内容，并去除前后空白字符。
	/// 3. 如果未找到节点，返回 null。
	/// </remarks>
	private static string? GetInnerText(HtmlDocument document, string xpath)
	{
		// 使用 HtmlAgilityPack 解析HTML内容，通过XPath选择器提取出指定节点
		var node = document.DocumentNode.SelectSingleNode(xpath);

		// 如果找到节点，返回节点的内部文本内容，并去除前后空白字符
		return node?.InnerText.Trim();
	}
}
