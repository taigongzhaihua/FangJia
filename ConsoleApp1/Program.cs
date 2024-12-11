using HtmlAgilityPack;

namespace ConsoleApp1;

internal class Program
{
    private static async Task Main()
    {
        const string url = "https://zhongyi.wiki/zyfangji/";

        try
        {
            // Download the webpage content
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            // Load the HTML document
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Find the specific container
            var container = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='article-container']");

            if (container == null)
            {
                Console.WriteLine("Could not find the specified container.");
                return;
            }

            // Extract the h2 and ol/li structure
            var results = new List<(string Category, string FormulaName)>();

            var h2Nodes = container.SelectNodes("./h2");
            if (h2Nodes != null)
            {
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
                        if (linkNodes != null)
                        {
                            results.AddRange(linkNodes.Select(linkNode => linkNode.InnerText.Trim()).Select(formulaName => (subCategory, formulaName)));
                        }
                    }
                }
            }

            // Print results
            foreach (var (category, formulaName) in results)
            {
                Console.WriteLine($"Category: {category}, Formula: {formulaName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}