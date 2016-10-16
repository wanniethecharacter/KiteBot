using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace KiteBot.Commands
{
    public static class Upcoming
    {
        public static string UpcomingUrl = "http://www.giantbomb.com/upcoming_json";

        public static void RegisterUpcomingCommand(DiscordClient client)
        {
            Console.WriteLine("Registering Game Command");

            client.GetService<CommandService>().CreateCommand("upcoming")
                    .Description("Upcoming Giantbomb.com")
                    .Do(async e =>
                    {
                        var json = await TestDownload();
                        string output = "";
                        output += json.liveNow + "\n";
                        output += string.Join("\n", json.upcoming.ToList());
                        await e.Channel.SendMessage(output);
                    });
        }

        public static async Task<GbUpcoming> TestDownload()
        {
            return JsonConvert.DeserializeObject<GbUpcoming>(await GetXDocumentFromUrl(UpcomingUrl));
        }

        private static async Task<string> GetXDocumentFromUrl(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent",
                    $"Bot for fetching livestreams and new content for the GiantBomb Shifty Discord Server. GETs every 2 minutes.");
                var jsonresult = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
                return jsonresult;
            }
        }

        public class GbUpcoming
        {
            [JsonProperty("liveNow")]
            public LiveNow liveNow { get; set; }

            [JsonProperty("upcoming")]
            public Upcoming[] upcoming { get; set; }

            public class LiveNow
            {
                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("image")]
                public string Image { get; set; }

                public override string ToString()
                {
                    return $"{Title} is LIVE now on <http://www.giantbomb.com/chat> \r\n{Image}";
                }
            }

            public class Upcoming
            {
                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("image")]
                public string Image { get; set; }

                [JsonProperty("date")]
                public string Date { get; set; }

                [JsonProperty("premium")]
                public bool Premium { get; set; }

                public override string ToString()
                {
                    return Premium ? $"Upcoming Premium {Type} on {Date} PDT: \n{Title}" : $"Upcoming {Type} on {Date} PDT: \n{Title}";
                }
            }
        }
    }
}
