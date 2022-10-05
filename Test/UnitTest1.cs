using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Test;

public class UnitTest1
{
    [Fact]
    public async Task SearchTest()
    {
        await foreach (var i in JutSu.SearchAsync("Две звезды"))
        {
            Assert.Equal(i.Url, new Uri("https://jut.su/onmyouji/"));
        }
    }

    [Fact]
    public async Task GetEpisodes()
    {
        var anime = await JutSu.SearchAsync("Две звезды").FirstOrDefaultAsync();

        Assert.NotNull(anime);
        var episode = await anime!.GetEpisodesAsync().FirstOrDefaultAsync();

        Assert.Equal(episode!.Url!, new Uri("https://jut.su/onmyouji/episode-1.html"));
    }
}