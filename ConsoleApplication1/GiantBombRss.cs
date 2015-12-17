using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace KiteBot
{
	class GiantBombRss
	{
		private static readonly string[] GiantBombUrl = { "http://www.giantbomb.com/feeds/podcast/", 
															"http://www.giantbomb.com/podcast-xml/beastcast/", 
															"http://www.giantbomb.com/feeds/video/" };
		private static List<Feed> _feeds;

		public GiantBombRss()
		{
			_feeds = new List<Feed>(GiantBombUrl.Length);
			foreach (string url in GiantBombUrl)
			{
				Feed feed = new Feed(url);
				feed.FeedUpdated += (s, e) => Program.SendMessage((Feed) s, (Feed.UpdatedFeedEventArgs)e);
				_feeds.Add(feed);
			}
			Timer GBTimer = new Timer();
			GBTimer.Elapsed += new ElapsedEventHandler(UpdateFeeds);
			GBTimer.Interval = 300000;
			GBTimer.Enabled = true;
		}

		private void UpdateFeeds(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			foreach (Feed feed in _feeds)
			{
				feed.UpdateFeed();
			}
		}
	}

	class Feed
	{
		public event EventHandler FeedUpdated;
		private XElement _latestXElement;
		private string _url;
		private DateTime _pubDate;

		public Feed(string url)
		{
			_latestXElement = GetXDocumentFromUrl(url);
			_url = url;
			_pubDate = getGiantBombFormatDateTime(_latestXElement.Element("pubDate").Value);
		}

		public class UpdatedFeedEventArgs : EventArgs
		{
			public string Title { get; set; }
			public string Link { get; set; }
		}
		public void UpdateFeed()
		{
			var newXElement = GetXDocumentFromUrl(_url);
			DateTime newPubDate = getGiantBombFormatDateTime(newXElement.Element("pubDate").Value);
			if (newPubDate.CompareTo(_pubDate) >= 0)
			{
				_latestXElement = newXElement;
				_pubDate = newPubDate;
				UpdatedFeedEventArgs eArgs = new UpdatedFeedEventArgs
				{
					Title = newXElement.Element("title").Value,
					Link = newXElement.Element("link").Value
				};
				OnUpdatedFeed(eArgs);
			}
		}

		protected virtual void OnUpdatedFeed(EventArgs e)
		{
			EventHandler handler = FeedUpdated;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private XElement GetXDocumentFromUrl(string url)
		{
			XDocument document = XDocument.Load(url);
			//XmlReader xr = document.Root.Element("rss").CreateReader();//.Element("channel").Element("item");
			var x = document.XPathSelectElement(@"//rss/channel/item");
			return x;
		}

		private DateTime getGiantBombFormatDateTime(string dateTimeString)
		{
			string timeString = dateTimeString;//this is really ugly, but all the feeds use different ways to encode their timezones AND I JUST DONT CARE anymore.
			timeString = timeString.Replace(" PDT","").Replace(" PST","").Replace(" -0800","");
			return DateTime.ParseExact(timeString,
				"ddd, dd MMM yyyy HH:mm:ss",
				CultureInfo.InvariantCulture);
		}
	}
}
