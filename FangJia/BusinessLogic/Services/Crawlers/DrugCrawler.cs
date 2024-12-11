using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Data;
using HtmlAgilityPack;
using NLog;
using System.Net.Http;

namespace FangJia.BusinessLogic.Services.Crawlers;

public class DrugCrawler : ICrawler<Drug>
{
    private static readonly HttpClient HttpClient = new();
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string BaseUrl = "https://www.zhongyifangji.com";
    private const string PageUrl = "https://www.zhongyifangji.com/materials/index/p/";

    public async Task<List<Drug>> GetListAsync()
    {
        List<Drug> drugs = [];

        try
        {
            Logger.Info("Starting to crawl drug data...");
            for (var i = 1; i <= 21; i++)
            {
                var url = PageUrl + i;
                Logger.Info($"Fetching page {i}: {url}");
                var links = await GetLinksAsync(url);

                foreach (var link in links)
                {
                    Logger.Info($"Fetching drug details from: {link}");
                    var drug = await GetDrugDetailsAsync(link);
                    if (drug != null)
                    {
                        drugs.Add(drug);
                    }
                }
            }

            Logger.Info($"Crawling completed. Total drugs fetched: {drugs.Count}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error during crawling: {ex.Message}");
        }

        return drugs;
    }

    private static async Task<List<string>> GetLinksAsync(string pageUrl)
    {
        List<string> links = [];
        try
        {
            var html = await HttpClient.GetStringAsync(pageUrl);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var nodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'card shadow-sm border-0')]/a");
            if (nodes != null)
            {
                links = nodes.Select(node => BaseUrl + node.GetAttributeValue("href", "")).ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error fetching links from {pageUrl}: {ex.Message}");
        }
        return links;
    }

    private static async Task<Drug?> GetDrugDetailsAsync(string url)
    {
        try
        {
            var html = await HttpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var drug = new Drug
            {
                Name = GetInnerText(document, "//strong[text()='名称']/following-sibling::div"),
                EnglishName = GetInnerText(document, "//strong[text()='英文名']/following-sibling::div"),
                LatinName = GetInnerText(document, "//strong[text()='拉丁名']/following-sibling::div"),
                Category = GetInnerText(document, "//strong[text()='分类']/following-sibling::div"),
                Origin = GetInnerText(document, "//strong[text()='产地']/following-sibling::div"),
                Properties = GetInnerText(document, "//strong[text()='性状']/following-sibling::div"),
                Quality = GetInnerText(document, "//strong[text()='品质']/following-sibling::div"),
                Taste = GetInnerText(document, "//strong[text()='性味']/following-sibling::div"),
                Effect = GetInnerText(document, "//strong[text()='功效']/following-sibling::div"),
                Notes = GetInnerText(document, "//strong[text()='注解']/following-sibling::div"),
                Processed = GetInnerText(document, "//strong[text()='炙品']/following-sibling::div"),
                Source = GetInnerText(document, "//strong[text()='来源']/following-sibling::div"),
                DrugImage = await GetDrugImageAsync(document)
            };
            return drug;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error fetching drug details from {url}: {ex.Message}");
            return null;
        }
    }

    private static async Task<DrugImage> GetDrugImageAsync(HtmlDocument document)
    {
        var drugImage = new DrugImage();

        try
        {
            var imgNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'col-6 d-none d-lg-block')]/img");
            Logger.Debug($"{imgNode}");
            if (imgNode != null)
            {
                var imgUrl = imgNode.GetAttributeValue("src", "");
                Logger.Debug($"imgUrl = {imgUrl}");
                if (!string.IsNullOrWhiteSpace(imgUrl))
                {
                    imgUrl = imgUrl.StartsWith("http") ? imgUrl : BaseUrl + imgUrl;

                    // Fetch image content
                    Logger.Info($"Fetching image: {imgUrl}");
                    var imgBytes = await HttpClient.GetByteArrayAsync(imgUrl);
                    drugImage.Image = imgBytes;
                }
            }
            else
            {
                Logger.Info("cannot found //div[@id='solutionmod-pic-section']/img");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error fetching image: {ex.Message}");
        }

        return drugImage;
    }

    private static string? GetInnerText(HtmlDocument document, string xpath)
    {
        var node = document.DocumentNode.SelectSingleNode(xpath);
        return node?.InnerText.Trim();
    }
}
