using System.Net;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;

namespace KiteBot
{
    public class LivestreamChecker
	{
		public static string ApiCallUrl;
        public static int RefreshRate;
		private static Timer _chatTimer;//Garbage collection doesnt like local variables that only fire a couple times per hour
		private XElement _latestXElement;
		private static bool _isStreamRunning;

        public LivestreamChecker(string GBapi,int streamRefresh)
        {
            if (GBapi.Length > 0 && streamRefresh > 3000)
            {
                ApiCallUrl = "http://www.giantbomb.com/api/chats/?api_key=" + GBapi;
                RefreshRate = streamRefresh;
                _chatTimer = new Timer();
                _chatTimer.Elapsed += RefreshChatsApi;
                _chatTimer.Interval = streamRefresh;
                _chatTimer.AutoReset = true;
                _chatTimer.Enabled = true;
            }
        }

        private void RefreshChatsApi(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			RefreshChatsApi();
		}

		private void RefreshChatsApi()
		{
			_latestXElement = GetXDocumentFromUrl(ApiCallUrl);

			if (_isStreamRunning == false && !_latestXElement.Element("number_of_page_results").Value.Equals("0"))
			{
				_isStreamRunning = true;

				var stream = _latestXElement.Element("results").Element("stream");
				var title = deGiantBombifyer(stream.Element("title").Value);
				var deck = deGiantBombifyer(stream.Element("deck").Value);

				Program.Client.GetChannel(85842104034541568).SendMessage(title +": "+ deck + " is LIVE at http://www.giantbomb.com/chat/ you should maybe check it out");
			}
			else if (_isStreamRunning && _latestXElement.Element("number_of_page_results").Value.Equals("0"))
			{
				_isStreamRunning = false;
				Program.Client.GetChannel(85842104034541568).SendMessage("Show is over folks, if you need more Giant Bomb videos, maybe check this out: " + KiteChat.GetResponseUriFromRandomQlCrew());
			}
		}

		private string deGiantBombifyer(string s)
		{
			return s.Replace("<![CDATA[ ", "").Replace(" ]]>", "");
		}

		private XElement GetXDocumentFromUrl(string url)
		{
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", $"KiteBot/1.1, Discord Bot for the GiantBomb EvE online \"corp\" looking for livestreams. GETs endpoint every {RefreshRate / 1000} seconds.");
            XDocument document = XDocument.Load(client.OpenRead(url));
			return document.XPathSelectElement(@"//response");
		}
	}
}
