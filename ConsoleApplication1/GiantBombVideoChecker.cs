using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;

namespace KiteBot
{
    public class GiantBombVideoChecker
	{
		public static string ApiCallUrl;
		private static Timer _chatTimer;//Garbage collection doesnt like local variables that only fire a couple times per hour.
		//private XElement _latestXElement; Dont Need this anymore I think.
        private DateTime lastPublishTime;
        private bool firstTime = true;

        public GiantBombVideoChecker(string GBapi,int streamRefresh)
        {
            ApiCallUrl = $"http://www.giantbomb.com/api/promos/?api_key={GBapi}&field_list=name,deck,date_added,link,user";
            _chatTimer = new Timer();
            _chatTimer.Elapsed += RefreshVideosApi;
            _chatTimer.Interval = streamRefresh;
            _chatTimer.AutoReset = true;
            _chatTimer.Enabled = true;
        }

        private async void RefreshVideosApi(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			await RefreshVideosApi();
		}

		private async Task RefreshVideosApi()
		{
            var _latestXElement = GetXDocumentFromUrl(ApiCallUrl);
            IEnumerable<XElement> promos = _latestXElement?.Element("results")?.Elements("promo");

            IOrderedEnumerable<XElement> sortedXElements = promos.OrderByDescending(e => GetGiantBombFormatDateTime(e.Element("date_added")?.Value));

		    if (firstTime)
		    {
		        lastPublishTime = GetGiantBombFormatDateTime(sortedXElements.First().Element("date_added")?.Value);
		        firstTime = false;
		    }
		    else
		    {
		        foreach (XElement item in sortedXElements)
		        {
                    DateTime newPublishTime = GetGiantBombFormatDateTime(item?.Element("date_added")?.Value);
                    if (newPublishTime.CompareTo(lastPublishTime) > 0)
                    {
                        var title = deGiantBombifyer(item?.Element("name")?.Value);
                        var deck = deGiantBombifyer(item?.Element("deck")?.Value);
                        var link = deGiantBombifyer(item?.Element("link")?.Value);
                        var user = deGiantBombifyer(item?.Element("user")?.Value);
                        lastPublishTime = newPublishTime;

                        await
                            Program.Client.GetChannel(85842104034541568)
                                .SendMessage(title + ": " + deck + Environment.NewLine + "by: " + user +
                                             Environment.NewLine + link);
                    }
                }
            }
		}
        private DateTime GetGiantBombFormatDateTime(string dateTimeString)
        {
            string timeString = dateTimeString;
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
