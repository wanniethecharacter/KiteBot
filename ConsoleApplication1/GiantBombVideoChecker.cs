using System;
using System.Globalization;
using System.Net;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;

namespace KiteBot
{
    public class GiantBombVideoChecker
	{
		public static string ApiCallUrl;
		private static Timer _chatTimer;//Garbage collection doesnt like local variables that only fire a couple times per hour
		private XElement _latestXElement;
        private DateTime lastPublishTime;
        private bool firstTime = true;

        public GiantBombVideoChecker(string GBapi,int streamRefresh)
        {
            ApiCallUrl = "http://www.giantbomb.com/api/promos/?api_key=" + GBapi;
            _chatTimer = new Timer();
            _chatTimer.Elapsed += RefreshVideosApi;
            _chatTimer.Interval = streamRefresh;
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
            var promo = _latestXElement.Element("results")?.Element("promo");
		    var promos = _latestXElement.Element("results")?.Elements("promo");
		    foreach (XElement item in promos)
		    {
                DateTime newPublishTime = GetGiantBombFormatDateTime(item?.Element("date_added")?.Value);
                if (newPublishTime.CompareTo(lastPublishTime) > 0)
		        {
		            if (firstTime)
		            {
		                lastPublishTime = newPublishTime;
		            }
		            else
		            {
                        var title = deGiantBombifyer(item?.Element("name")?.Value);
                        var deck = deGiantBombifyer(item?.Element("deck")?.Value);
                        var link = deGiantBombifyer(item?.Element("link")?.Value);
                        var user = deGiantBombifyer(item?.Element("user")?.Value);
                        lastPublishTime = newPublishTime;

                        Program.Client.GetChannel(85842104034541568).SendMessage(title + ": " + deck + Environment.NewLine + "by: " + user + Environment.NewLine + link);
                    }
		        }
		    }
            firstTime = false;
        }
        private DateTime GetGiantBombFormatDateTime(string dateTimeString)
        {
            string timeString = dateTimeString;
            //This is really ugly, but all the feeds use different ways to encode their timezones and I JUST DONT CARE anymore.
            //Since the feeds are atleast consistent within that particular feed, this shouldn't cause a conflict when comparing timeDates
            return DateTime.ParseExact(timeString,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture);
        }

        private string deGiantBombifyer(string s)
		{
			return s.Replace("<![CDATA[ ", "").Replace(" ]]>", "");
		}

		private XElement GetXDocumentFromUrl(string url)
        {
		    try
		    {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "LassieMEKiteBot/0.9 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                XDocument document = XDocument.Load(client.OpenRead(url));
		        return document.XPathSelectElement(@"//response");
		    }
		    catch (Exception)
		    {
		        return GetXDocumentFromUrl(url);
		    }
        }
	}
}
