using FangJia.BusinessLogic.Interfaces;
using HtmlAgilityPack;
using NLog;
using System.Net.Http;

namespace FangJia.BusinessLogic.Services.Crawlers;

public class FangjiCrawler : ICrawler<(string Category, string FormulaName)>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string Url = "https://zhongyi.wiki/zyfangji/";

    public async Task<List<(string Category, string FormulaName)>> GetListAsync()
    {
        var results = new List<(string Category, string FormulaName)>();
        try
        {
            Logger.Info($"Starting to fetch data from URL: {Url}");

            // Download the webpage content
            using var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(Url);
            Logger.Info("Successfully fetched data from URL.");

            // Load the HTML document
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Find the specific container
            var container = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='article-container']");
            if (container == null)
            {
                Logger.Info("Could not find the specified container.");
                return results;
            }

            // Extract the h2 and ol/li structure
            var h2Nodes = container.SelectNodes("./h2");
            if (h2Nodes != null)
            {
                Logger.Info($"Found {h2Nodes.Count} categories.");
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
                            Logger.Info($"Extracted: {subCategory} - {formulaName}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occurred during data fetching.");
        }

        return results;
    }
}