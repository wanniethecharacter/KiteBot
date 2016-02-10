using System;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;
using KiteBot.Properties;

namespace KiteBot
{
    public class GiantBombVideoChecker
	{
		public static string ApiCallUrl;
		private static Timer _chatTimer;//Garbage collection doesnt like local variables that only fire a couple times per hour
		private XElement _latestXElement;
		private int totalVideos;

		public GiantBombVideoChecker()
		{
			ApiCallUrl = "http://www.giantbomb.com/api/videos/?api_key=" + auth.Default.GiantBombAPI;
			_chatTimer = new Timer();
			_chatTimer.Elapsed += RefreshVideosApi;
			_chatTimer.Interval = 60000;//1 minute
			_chatTimer.AutoReset = true;
			_chatTimer.Enabled = true;
		}

		private void RefreshVideosApi(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			RefreshVideosApi();
		}

		private void RefreshVideosApi()
		{
		    _latestXElement = GetXDocumentFromUrl(ApiCallUrl);
            int newVideoCount;
		    if (totalVideos == 0)
		    {
		        if (int.TryParse(_latestXElement?.Element("number_of_total_results").Value, out newVideoCount))
		        {
		            totalVideos = newVideoCount;
		        }
		    }
		    else
		    {
                if (int.TryParse(_latestXElement?.Element("number_of_total_results").Value, out newVideoCount))
                {
                    if (newVideoCount > totalVideos)
                    {
                        var video = _latestXElement.Element("results")?.Element("video");
                        var title = deGiantBombifyer(video?.Element("name")?.Value);
                        var deck = deGiantBombifyer(video?.Element("deck")?.Value);
                        var link = deGiantBombifyer(video?.Element("site_detail_url")?.Value);

                        Program.Client.SendMessage(Program.Client.GetChannel(85842104034541568),
                        title + ": " + deck + Environment.NewLine + link);
                    }
                }
            }
		}

		private string deGiantBombifyer(string s)
		{
			return s.Replace("<![CDATA[ ", "").Replace(" ]]>", "");
		}

		private XElement GetXDocumentFromUrl(string url)
		{
		    try
		    {
		        XDocument document = XDocument.Load(url);
		        return document.XPathSelectElement(@"//response");
		    }
		    catch (Exception e)
		    {
		        return null;
		    }
		}
	}
}
