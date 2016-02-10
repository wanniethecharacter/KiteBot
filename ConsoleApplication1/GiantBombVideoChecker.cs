using System;
using System.Globalization;
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
        private DateTime lastPublishTime;
        private bool firstTime = true;

		public GiantBombVideoChecker()
		{
			ApiCallUrl = "http://www.giantbomb.com/api/promos/?api_key=" + auth.Default.GiantBombAPI;
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
            var promo = _latestXElement.Element("results")?.Element("promo");
            DateTime newPublishTime = GetGiantBombFormatDateTime(promo?.Element("date_added")?.Value);
		    if (firstTime || newPublishTime.Equals(lastPublishTime))
		    {
                lastPublishTime = newPublishTime;
                firstTime = false;
            }
            else
            {
                var title = deGiantBombifyer(promo?.Element("name")?.Value);
                var deck = deGiantBombifyer(promo?.Element("deck")?.Value);
                var link = deGiantBombifyer(promo?.Element("link")?.Value);
                var user = deGiantBombifyer(promo?.Element("user")?.Value);
                lastPublishTime = newPublishTime;

                Program.Client.SendMessage(Program.Client.GetChannel(85842104034541568),
                title + ": " + deck + Environment.NewLine + "by: " + user + Environment.NewLine + link);
            }
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
		        XDocument document = XDocument.Load(url);
		        return document.XPathSelectElement(@"//response");
		    }
		    catch (Exception e)
		    {
		        return GetXDocumentFromUrl(url);
		    }
        }
	}
}
