using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace JutSuCS;
public static class JutSu
{
    public static HttpClient CreateWebSession()
    {
        HttpClient cli = new();
        cli.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X x.y; rv:42.0) Gecko/20100101 Firefox/42.0");
        return cli;
    }

    public static string? DecodeString(string str)
    {
        var utf8 = Encoding.UTF8;
        var win1251 = CodePagesEncodingProvider.Instance.GetEncoding(1251);
        return win1251?.GetString(Encoding.Convert(win1251, utf8, win1251.GetBytes(str)));
    }
    public static async Task<HtmlDocument> CreateHtmlDocumentAsync(HttpClient? session, string? url)
    {
        var videoResponse = await session!.GetAsync(url!);
        var parsedVideo = new HtmlDocument();
        parsedVideo.Load(await videoResponse.Content.ReadAsStreamAsync());
        return parsedVideo;
    }
    public static async Task<HtmlDocument> CreateHtmlDocumentAsync(HttpClient? session, Uri? url)
    {
        return await CreateHtmlDocumentAsync(session, url!.AbsoluteUri);
    }
    public static async IAsyncEnumerable<JutAnime> SearchAsync(string searchText)
    {
        var session = CreateWebSession();
        session.DefaultRequestHeaders.Add("Referer", "http://jut.su/anime/");

        var searchResponse = await session.PostAsync("https://jut.su/anime/", 
            new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new ("ajax_load", "yes"),
            new ("start_from_page", "1"),
            new ("show_search", searchText),
            new ("anime_of_user", "")
        }));

        searchResponse.EnsureSuccessStatusCode();

        var document = new HtmlDocument();
        document.Load(await searchResponse.Content.ReadAsStreamAsync());

        var animes = document.DocumentNode.Descendants("div").Where(n => n.HasClass("all_anime_global"));

        foreach (var i in animes)
        {
            var contentBody = i.Descendants("div").First(n => n.HasClass("all_anime"));

            var url = $"https://jut.su{i.Descendants("a").First().GetAttributeValue("href", null)}";
            var styleContent = contentBody.Descendants().First(n => n.HasClass("all_anime_image"))
                .GetAttributeValue("style", "");
            var img = Regex.Match(
                styleContent,
                @"'(https:\/\/.*?)'");
            var name = contentBody.Descendants().First(n => n.HasClass("aaname")).InnerText;

            yield return new JutAnime
            {
                Image = new Uri(img.Groups[1].Value),
                Name = name,
                Url = new Uri(url)
            };
        }
    }
}
