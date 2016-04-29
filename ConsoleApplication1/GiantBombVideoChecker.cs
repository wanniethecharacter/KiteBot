using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;
using Timer = System.Timers.Timer;

namespace KiteBot
{
    public class GiantBombVideoChecker
	{
		public static string ApiCallUrl;
        public static int RefreshRate;
		private static Timer _chatTimer;//Garbage collection doesnt like local variables that only fire a couple times per hour.
		//private XElement _latestXElement; Dont Need this anymore I think.
        private DateTime lastPublishTime;
        private bool firstTime = true;

        public GiantBombVideoChecker(string GBapi,int streamRefresh)
        {
            ApiCallUrl = $"http://www.giantbomb.com/api/promos/?api_key={GBapi}&field_list=name,deck,date_added,link,user";
            RefreshRate = streamRefresh;
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
            var latestXElement = GetXDocumentFromUrl(ApiCallUrl);
            IEnumerable<XElement> promos = latestXElement?.Element("results")?.Elements("promo");

            IOrderedEnumerable<XElement> sortedXElements = promos.OrderBy(e => GetGiantBombFormatDateTime(e.Element("date_added")?.Value));

		    if (firstTime)
		    {
		        lastPublishTime = GetGiantBombFormatDateTime(sortedXElements.Last().Element("date_added")?.Value);
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
                client.Headers.Add("user-agent", $"KiteBot/1.1, Discord Bot for the GiantBomb EvE online \"corp\" looking for new videos, articles and podcasts. GETs endpoint every {RefreshRate/1000} seconds. <3 u edgework");
                XDocument document = XDocument.Load(client.OpenRead(url));
		        return document.XPathSelectElement(@"//response");
		    }
		    catch (Exception ex)
		    {
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
                return GetXDocumentFromUrl(url);
		    }
        }
	}
}
