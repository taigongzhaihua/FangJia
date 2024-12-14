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
	private static readonly HttpClient HttpClient = new();
	private static readonly Logger     Logger     = LogManager.GetCurrentClassLogger();
	private const           string     BaseUrl    = "https://www.zhongyifangji.com";
	private const           string     PageUrl    = "https://www.zhongyifangji.com/materials/index/p/";

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
		var progressReport = new CrawlerProgress(21, 0, true);
		// 初始化一个空的药品列表
		List<Drug> drugs = [];
		progress.Report(progressReport);
		try
		{
			Logger.Info("Starting to crawl drug data...");
			List<string> links = [];
			// 从第1页到第21页，依次获取每页的URL
			for (var i = 1; i <= 21; i++)
			{
				var url = PageUrl + i;
				var log = $@"Fetching page {i}: {url}";
				Logger.Info(log);
				progressReport.AddLog(log);
				progress.Report(progressReport);

				// 调用 GetLinksAsync 方法获取该页面上所有药品的链接
				links                      = [..links, ..await GetLinksAsync(url)];
				progressReport.TotalLength = links.Count + 21;
				progressReport.UpdateProgress(i);
				progress.Report(progressReport);
			}

			// 对于每个药品链接，调用 GetDrugDetailsAsync 方法获取药品的详细信息
			foreach (var link in links)
			{
				var log = $@"Fetching drug details from: {link}";
				Logger.Info(log);
				progressReport.AddLog(log);
				var drug = await GetDrugDetailsAsync(link);

				// 如果药品详细信息获取成功，将其添加到药品列表中
				if (drug != null)
				{
					drugs.Add(drug);
				}

				progressReport.UpdateProgress(progressReport.CurrentProgress + 1);
				progress.Report(progressReport);
			}

			// 爬取完成后，记录总药品数量
			Logger.Info($@"Crawling completed. Total drugs fetched: {drugs.Count}");
			progressReport.AddLog($@"Crawling completed. Total drugs fetched: {drugs.Count}");
			progressReport.IsRunning = false;
			progress.Report(progressReport);
		}
		catch (Exception ex)
		{
			// 记录爬取过程中的错误
			Logger.Error($"Error during crawling: {ex.Message}");
		}

		// 返回药品列表
		return drugs;
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
	private static async Task<List<string>> GetLinksAsync(string pageUrl)
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
			}
		}
		catch (Exception ex)
		{
			// 如果在过程中发生异常，记录错误信息
			Logger.Error($@"Error fetching links from {pageUrl}: {ex.Message}");
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
	private static async Task<Drug?> GetDrugDetailsAsync(string url)
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
	private static async Task<DrugImage> GetDrugImageAsync(HtmlDocument document)
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
				Logger.Debug($"图片URL = {imgUrl}");

				if (!string.IsNullOrWhiteSpace(imgUrl))
				{
					// 确保图片URL是完整的，如果不是，则拼接 BaseUrl
					imgUrl = imgUrl.StartsWith("http") ? imgUrl : BaseUrl + imgUrl;

					// 使用 HttpClient 下载图片内容
					Logger.Info($"正在获取图片: {imgUrl}");
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
