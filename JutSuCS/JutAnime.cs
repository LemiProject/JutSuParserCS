using System.Collections;
using HtmlAgilityPack;

namespace JutSuCS;

public class JutAnime
{
    public Uri? Url { get; set; }
    public string? Name { get; set; }
    public Uri? Image { get; set; }
    public async IAsyncEnumerable<JutEpisode> GetEpisodesAsync()
    {
        var session = JutSu.CreateWebSession();
        var parsedResponse = await JutSu.CreateHtmlDocumentAsync(session, Url!);

        foreach (var videoNode in parsedResponse.DocumentNode.Descendants().Where(n => n.HasClass("video")))
        {
            var url = $"https://jut.su{videoNode.GetAttributeValue("href", "")}";

            var parsedVideoResponse = await JutSu.CreateHtmlDocumentAsync(session, url);
            var name = parsedVideoResponse.DocumentNode.Descendants().First(n => n.HasClass("video_plate_title"))
                .Descendants("h2").First().InnerText;

            yield return new JutEpisode
            {
                Url = new Uri(url),
                Name = JutSu.DecodeString(name),
            };

            await Task.Delay(100);
        }
    }
}