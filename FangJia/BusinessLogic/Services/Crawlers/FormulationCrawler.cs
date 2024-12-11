using FangJia.BusinessLogic.Interfaces;
using FangJia.BusinessLogic.Models.Data;
using HtmlAgilityPack;
using NLog;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace FangJia.BusinessLogic.Services.Crawlers;

public class FormulationCrawler : ICrawler<Formulation>
{
    private static readonly HttpClient HttpClient = new();
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string BaseUrl = "https://www.zhongyifangji.com";

    public async Task<List<Formulation>> GetListAsync()
    {
        var formulations = new List<Formulation>();
        for (var i = 1; i <= 10; i++)
        {
            var url = $"{BaseUrl}/prescription/index/p/{i}";
            Logger.Info($"Fetching page {i}: {url}");
            var links = await GetLinksAsync(url);

            foreach (var link in links)
            {
                Logger.Info($"Fetching formulation details from: {link}");
                var formulation = await GetFormulationDetailsAsync(link);
                if (formulation != null)
                {
                    formulations.Add(formulation);
                }
            }
        }
        Logger.Info($"Completed fetching formulations. Total count: {formulations.Count}");
        return formulations;
    }

    private static async Task<List<string>> GetLinksAsync(string pageUrl)
    {
        var links = new List<string>();
        try
        {
            var html = await HttpClient.GetStringAsync(pageUrl);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var nodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'card shadow-sm border-0')]/a");
            if (nodes != null)
            {
                links = nodes.Select(node => BaseUrl + node.GetAttributeValue("href", "")).ToList();
                Logger.Info($"Found {links.Count} links on page: {pageUrl}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error fetching links from {pageUrl}: {ex.Message}");
        }
        return links;
    }

    private static async Task<Formulation?> GetFormulationDetailsAsync(string url)
    {
        try
        {
            var html = await HttpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var formulation = new Formulation
            {
                Name = GetInnerText(document, "//strong[text()='名称']/following-sibling::div"),
                Usage = GetInnerText(document, "//strong[text()='用法']/following-sibling::div"),
                Effect = GetInnerText(document, "//strong[text()='功用']/following-sibling::div"),
                Indication = GetInnerText(document, "//strong[text()='主治']/following-sibling::div"),
                Disease = GetInnerText(document, "//strong[text()='病机']/following-sibling::div"),
                Application = GetInnerText(document, "//strong[text()='运用']/following-sibling::div"),
                Supplement = GetInnerText(document, "//strong[text()='附方']/following-sibling::div"),
                Song = GetInnerText(document, "//strong[text()='方歌']/following-sibling::div"),
                Notes = GetInnerText(document, "//strong[text()='附注']/following-sibling::div"),
                Source = GetInnerText(document, "//strong[text()='出处']/following-sibling::div"),
                Compositions = GetCompositions(document),
                FormulationImage = (await GetImage(document))!
            };

            Logger.Info($"Successfully fetched formulation: {formulation.Name}");
            return formulation;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error fetching formulation details from {url}: {ex.Message}");
            return null;
        }
    }

    private static async Task<FormulationImage?> GetImage(HtmlDocument document)
    {
        try
        {
            var imageNode = document.DocumentNode.SelectSingleNode("//div[@id='solutionmod-pic-section']//img");
            if (imageNode != null)
            {
                var imageUrl = imageNode.GetAttributeValue("src", null);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    Logger.Info($"Fetching image from: {imageUrl}");
                    var imageBytes = await HttpClient.GetByteArrayAsync(imageUrl);
                    return new FormulationImage
                    {
                        Image = imageBytes
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error fetching image: {ex.Message}");
        }
        return null;
    }


    private static ObservableCollection<FormulationComposition> GetCompositions(HtmlDocument document)
    {
        var compositions = new ObservableCollection<FormulationComposition>();
        var tableRows = document.DocumentNode.SelectNodes("//table[@id='formula_table']//tr");
        if (tableRows == null) return compositions;
        foreach (var row in tableRows)
        {
            var cells = row.SelectNodes("td");
            if (cells is { Count: >= 4 })
            {
                compositions.Add(new FormulationComposition
                {
                    Position = cells[0]?.InnerText.Trim(),
                    DrugName = cells[1]?.InnerText.Trim(),
                    Effect = cells[2]?.InnerText.Trim(),
                    Notes = cells[3]?.InnerText.Trim()
                });
            }
        }
        Logger.Info($"Extracted {compositions.Count} compositions from formulation.");
        return compositions;
    }

    private static string? GetInnerText(HtmlDocument document, string xpath)
    {
        var node = document.DocumentNode.SelectSingleNode(xpath);
        return node?.InnerText.Trim();
    }
}