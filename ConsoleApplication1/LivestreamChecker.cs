using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Discord;
using Discord.WebSocket;
using Timer = System.Timers.Timer;

namespace KiteBot
{
    public class LivestreamChecker
	{
		public static string ApiCallUrl;
        public static int RefreshRate;
		private static Timer _chatTimer;//Garbage collection doesnt like local timers.
		private XElement _latestXElement;
		private static bool _wasStreamRunning;

        public LivestreamChecker(string gBapi,int streamRefresh)
        {
            if (gBapi.Length > 0 && streamRefresh > 3000)
            {
                ApiCallUrl = $"http://www.giantbomb.com/api/chats/?api_key={gBapi}&field_list=deck,title";
                RefreshRate = streamRefresh;
                _chatTimer = new Timer();
                _chatTimer.Elapsed += async (s, e) => await RefreshChatsApi();
                _chatTimer.Interval = streamRefresh;
                _chatTimer.AutoReset = true;
                _chatTimer.Enabled = true;
            }
        }

        public void Restart()
        {
            if (_chatTimer == null)
            {
                Console.WriteLine("_chatTimer eaten by GC");
                Environment.Exit(-1);
            }
            else if (_chatTimer.Enabled == false)
            {
                Console.WriteLine("Was off, turning LiveStream back on.");
                _chatTimer.Start();
                if (_chatTimer.AutoReset == false)
                {
                    Console.WriteLine("AutoReset was off");
                    _chatTimer.AutoReset = true;
                }
            }
        }

        private async Task RefreshChatsApi()
        {
            try
            {
                if (Program.Client.Guilds.Any())
                {
                    try
                    {
                        _latestXElement = await GetXDocumentFromUrl(ApiCallUrl,0).ConfigureAwait(false);
                        var numberOfResults = _latestXElement.Element("number_of_page_results")?.Value;
                        var channel = (ITextChannel) Program.Client.GetChannel(85842104034541568);

                        if (_wasStreamRunning == false && !numberOfResults.Equals("0"))
                        {
                            _wasStreamRunning = true;

                            var stream = _latestXElement.Element("results")?.Element("stream");
                            var title = deGiantBombifyer(stream?.Element("title")?.Value);
                            var deck = deGiantBombifyer(stream?.Element("deck")?.Value);

                            await channel.SendMessageAsync(title + ": " + deck +
                                                 " is LIVE at http://www.giantbomb.com/chat/ NOW, check it out!");
                        }
                        else if (_wasStreamRunning && numberOfResults.Equals("0"))
                        {
                            _wasStreamRunning = false;

                            await channel.SendMessageAsync("Show is over folks, if you need more Giant Bomb videos, check this out: " +
                                        KiteChat.GetResponseUriFromRandomQlCrew());
                        }

                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("Livestreamchecker timed out. Restarting Timer.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LivestreamChecker sucks: {ex} \n {ex.Message}");
                var ownerDMChannel = await Program.Client.GetDMChannelAsync(85817630560108544);
                if (ownerDMChannel != null)
                    await ownerDMChannel.SendMessageAsync($"LivestreamChecker threw an {ex.GetType()}, check the logs").ConfigureAwait(false);
            }
        }

        private string deGiantBombifyer(string s)
		{
			return s.Replace("<![CDATA[ ", "").Replace(" ]]>", "");
		}

        private async Task<XElement> GetXDocumentFromUrl(string url, int retry)
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent",
                    $"Bot for fetching livestreams and new content for the GiantBomb Shifty Discord Server. GETs every {RefreshRate / 1000 / 60} minutes.");
                XDocument document = XDocument.Load(await client.OpenReadTaskAsync(url).ConfigureAwait(false));
                return document.XPathSelectElement(@"//response");
            }
            catch (Exception)
            {
                if (++retry < 2)
                {
                    await Task.Delay(10000);
                    return await GetXDocumentFromUrl(url,retry).ConfigureAwait(false);
                }
                throw new TimeoutException();
            }
        }
    }
}
