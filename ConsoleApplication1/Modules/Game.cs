using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord.Commands;
using KiteBot.Json.GiantBomb.Search;
using Newtonsoft.Json;

namespace KiteBot.Modules
{
    public class Game : ModuleBase
    {
        public static string ApiCallUrl =
                $"http://www.giantbomb.com/api/search/?api_key={Program.Settings.GiantBombApiKey}&field_list=deck,image,name,original_release_date,platforms,site_detail_url&format=json&query=\"";

        [Command("game")]
        [Summary("Finds a game in the Giantbomb games database")]
        public async Task GameCommand([Remainder] string gameTitle)
        {
            if (!string.IsNullOrWhiteSpace(gameTitle))
            {
                var s = await GetGamesEndpoint(gameTitle, 0);
                await ReplyAsync(s.Results.FirstOrDefault()?.ToString());
            }
            else
            {
                await ReplyAsync("Empty game name given, please specify a game title");
            }
        }

        private async Task<Search> GetGamesEndpoint(string gameTitle, int retry)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("user-agent",
                        $"Bot for fetching livestreams, new content and the occasional wiki page for the GiantBomb Shifty Discord Server.");
                    Search search = JsonConvert.DeserializeObject<Search>(await client.DownloadStringTaskAsync(ApiCallUrl+gameTitle+"\""));
                    return search;
                }
            }
            catch (Exception)
            {
                if (++retry < 3)
                {
                    await Task.Delay(10000);
                    return await GetGamesEndpoint(gameTitle, retry);
                }
                throw new TimeoutException();
            }
        }
    }
}