using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;
using KiteBot.Properties;

namespace KiteBot
{
	public class LivestreamChecker
	{
		public static string ApiCallUrl;
		private static Timer _chatTimer;//Garbage collection doesnt like local variables that only fire a couple times per hour
		private XElement _latestXElement;
		private bool isStreamRunning;

		public LivestreamChecker()
		{
			ApiCallUrl = "http://www.giantbomb.com/api/chats/?api_key=" + auth.Default.GiantBombAPI;
			_chatTimer = new Timer();
			_chatTimer.Elapsed += RefreshChatsApi;
			_chatTimer.Interval = 120000;//2 minutes 2*60*1000=120 000
			_chatTimer.AutoReset = true;
			_chatTimer.Enabled = true;
		}

		private void RefreshChatsApi(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			RefreshChatsApi();
		}

		private void RefreshChatsApi()
		{
			_latestXElement = GetXDocumentFromUrl(ApiCallUrl);

			if (isStreamRunning == false && !_latestXElement.Value.Equals("0"))
			{
				isStreamRunning = true;
				Program.Client.SendMessage(Program.Client.GetChannel(85842104034541568),
			    "Chat is LIVE at http://www.giantbomb.com/chat/ you should maybe check it out");
			}
			else if (isStreamRunning && _latestXElement.Value.Equals("0"))
			{
				isStreamRunning = false;
				Program.Client.SendMessage(Program.Client.GetChannel(85842104034541568),
			    "Show is over folks, if you need more Giant Bomb videos, maybe check this out: " + KiteChat.GetResponseUriFromRandomQlCrew());
			}
		}
		private XElement GetXDocumentFromUrl(string url)
		{
			XDocument document = XDocument.Load(url);
			return document.XPathSelectElement(@"//response/number_of_page_results");
		}
	}
}
