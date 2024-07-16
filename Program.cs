using System.Data;
using System.Text;
using HtmlAgilityPack;
class Program
{
    internal static readonly string url = "https://coolpc.com.tw/evaluate.php";
    internal static readonly string[] replace = ["◆", "★", " 熱賣"];
    internal static readonly string[] separator = [", $", "↘"];
    static async Task Main()
    {
        HtmlDocument html = await GetHtmlDocument();
        var options = GetOptions(html);
        options.ForEach(d => {
            Console.WriteLine($"類別：{d.Item1}");
            Console.WriteLine();
            var item = GetItem(html, d.Item2);
            item.ForEach(i =>
            {
                Console.WriteLine($"品名：{i.Item1}");
                Console.WriteLine($"價格：{i.Item2}");
            });
            Console.WriteLine();
        });
    }
    private static async Task<HtmlDocument> GetHtmlDocument()
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var byteArray = await response.Content.ReadAsByteArrayAsync();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var responseBody = Encoding.GetEncoding("big5").GetString(byteArray);
        var html = new HtmlDocument();
        html.LoadHtml(responseBody);
        return html;
    }
    private static List<Tuple<string, string>> GetOptions(HtmlDocument html)
    {
        var values = new List<Tuple<string, string>>();
        try
        {
            var tableElement = html.DocumentNode.SelectSingleNode("//table[@id='Tfix']");
            var rows = tableElement.SelectNodes(".//tr");

            foreach (var row in rows)
            {
                var labelNode = row.SelectSingleNode(".//td[@class='t']");
                var selectNode = row.SelectSingleNode(".//select");

                if (labelNode is null || selectNode is null || selectNode.Attributes["name"] is null)
                    continue;
                var labelText = labelNode.InnerText.Trim();
                var selectName = selectNode.Attributes["name"].Value;
                values.Add(new Tuple<string, string>(labelText, selectName));

            }
        }
        catch { return []; }
        return values;
    }
    private static List<Tuple<string, int>> GetItem(HtmlDocument html, string name)
    {
        var values = new List<Tuple<string, int>>();
        try
        {
            var selectElement = html.DocumentNode.SelectSingleNode($"//select[@name='{name}']");
            if (selectElement is null)
                return [];
            var options = selectElement.SelectNodes(".//option");

            foreach (var option in options)
            {
                if (option.Attributes["disabled"] is not null || option.ParentNode.Name is "optgroup")
                    continue;
                var itemText = option.InnerText.Trim();
                itemText = itemText.Replace(replace[0], "").Replace(replace[1], "").Replace(replace[2], "");
                var items = itemText.Split('\n').Select(i => i.Trim()).Where(i => !i.StartsWith('❤') && !i.StartsWith("　　") && !string.IsNullOrWhiteSpace(i));

                foreach (var item in items)
                {
                    if (!item.Contains(separator[0]))
                        continue;
                    var parts = item.Split(separator[0], StringSplitOptions.None);
                    var itemName = parts[0].Trim();
                    var itemPrice = parts[1].Trim().Replace(",", "");

                    if (itemPrice.Contains(separator[1]))
                        itemPrice = itemPrice.Split(separator[1], StringSplitOptions.None).Last().Replace("$", "").Trim();
                    if (int.TryParse(itemPrice, out var price) && price != 1)
                        values.Add(new Tuple<string, int>(itemName, price));
                }
            }
        }
        catch { return []; }
        return values;
    }
}
