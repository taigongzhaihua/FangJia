using System.Diagnostics.CodeAnalysis;
using FangJia.BusinessLogic.Interfaces;
using HtmlAgilityPack;
using NLog;
using System.Net.Http;
using FangJia.BusinessLogic.Models;

namespace FangJia.BusinessLogic.Services.Crawlers;

/// <summary>
/// 方剂爬虫服务类，实现 ICrawler&lt;Tuple&lt;string, string&gt;&gt; 接口，用于从指定网站爬取方剂信息。
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class FangjiCrawler : ICrawler<(string Category, string FormulaName)>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string Url = "https://zhongyi.wiki/zyfangji/";

    /// <summary>
    /// 获取方剂列表的异步方法。
    /// 该方法通过爬取指定网站的页面，获取方剂的详细信息，并返回一个包含所有方剂的列表。
    /// </summary>
    /// <returns>包含所有方剂的列表</returns>
    /// <remarks>
    /// 实现步骤：
    /// 1. 初始化一个空的结果列表。
    /// 2. 下载网页内容。
    /// 3. 加载HTML文档。
    /// 4. 查找特定的容器。
    /// 5. 提取h2和ol/li结构。
    /// 6. 对于每个方剂，提取分类和名称信息。
    /// 7. 将分类和名称信息添加到结果列表中。
    /// </remarks>
    public async Task<List<(string Category, string FormulaName)>> GetListAsync(IProgress<CrawlerProgress> progress)
    {
        var results = new List<(string Category, string FormulaName)>();
        try
        {
            // 开始从指定URL获取数据
            Logger.Info($"开始从URL获取数据: {Url}");

            // 使用 HttpClient 下载网页内容
            using var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(Url);
            Logger.Info("成功从URL获取数据。");

            // 加载HTML文档
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // 查找特定的容器
            var container = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='article-container']");
            if (container == null)
            {
                Logger.Info("未找到指定的容器。");
                return results;
            }

            // 提取h2和ol/li结构
            var h2Nodes = container.SelectNodes("./h2");
            if (h2Nodes != null)
            {
                Logger.Info($"找到 {h2Nodes.Count} 个分类。");
                foreach (var h2Node in h2Nodes)
                {
                    var olNode = h2Node.SelectSingleNode("following-sibling::ol[1]");
                    var liNodes = olNode?.SelectNodes(".//li");
                    if (liNodes == null) continue;
                    foreach (var liNode in liNodes)
                    {
                        var subCategoryNode = liNode.SelectSingleNode("./strong");
                        if (subCategoryNode == null) continue;
                        var subCategory = subCategoryNode.InnerText.Trim();
                        var linkNodes = liNode.SelectNodes(".//a");
                        if (linkNodes == null) continue;
                        foreach (var linkNode in linkNodes)
                        {
                            var formulaName = linkNode.InnerText.Trim();
                            results.Add((subCategory, formulaName));
                            Logger.Info($"提取: {subCategory} - {formulaName}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 捕获并记录数据获取过程中发生的异常
            Logger.Error(ex, "数据获取过程中发生错误。");
        }

        // 返回提取的方剂列表
        Logger.Info("方剂列表提取完成，返回结果。");
        return results;
    }
}
