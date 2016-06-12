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
        private int _retry;

        public GiantBombVideoChecker(string GBapi,int videoRefresh)
        {
            if (GBapi.Length > 0 && videoRefresh > 3000)
            {
                ApiCallUrl =
                    $"http://www.giantbomb.com/api/promos/?api_key={GBapi}&field_list=name,deck,date_added,link,user";
                RefreshRate = videoRefresh;
                _chatTimer = new Timer();
                _chatTimer.Elapsed += RefreshVideosApi;
                _chatTimer.Interval = videoRefresh;
                _chatTimer.AutoReset = true;
                _chatTimer.Enabled = true;
            }
        }

        public void Restart()
        {
            if (_chatTimer == null)
            {
                Console.WriteLine("VideoChecker _chatTimer eaten by GC");
                Environment.Exit(-1);
            }
            else if (_chatTimer.Enabled == false)
            {
                Console.WriteLine("Was off, turning VideoChecker back on.");
                _chatTimer.Start();
                if (_chatTimer.AutoReset == false)
                {
                    Console.WriteLine("AutoReset was off");
                    _chatTimer.AutoReset = true;
                }
            }
        }

        private async void RefreshVideosApi(object sender, ElapsedEventArgs elapsedEventArgs)
		{
            try
            {
                await RefreshVideosApi();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
            }
		}

		private async Task RefreshVideosApi()
		{
            var latestXElement = await GetXDocumentFromUrl(ApiCallUrl);
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

        private async Task<XElement> GetXDocumentFromUrl(string url)
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent",
                    $"KiteBot/1.1, Discord Bot for the GiantBomb EvE online \"corp\" looking for new videos, articles and podcasts. GETs endpoint every {RefreshRate/1000} seconds. <3 u edgework");
                XDocument document = XDocument.Load(await client.OpenReadTaskAsync(url).ConfigureAwait(false));
                return document.XPathSelectElement(@"//response");
            }
            catch (Exception)
            {
                _retry++;
                if (_retry < 3)
                {
                    await Task.Delay(10000);
                    return await GetXDocumentFromUrl(url).ConfigureAwait(false);
                }
                throw new TimeoutException();
            }
        }
	}
}
