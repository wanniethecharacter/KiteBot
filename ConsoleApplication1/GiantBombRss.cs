using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;

namespace KiteBot
{
	public class GiantBombRss
	{
		private static readonly string[] GiantBombUrl = { "http://www.giantbomb.com/feeds/mashup/" };//,"http://www.giantbomb.com/feeds/podcast/", "http://www.giantbomb.com/podcast-xml/beastcast/", "http://www.giantbomb.com/feeds/video/
		private static List<Feed> _feeds;
		private static Timer GBTimer;//Garbage collection doesnt like local variables

		public GiantBombRss()
		{
			_feeds = new List<Feed>(GiantBombUrl.Length);
			foreach (string url in GiantBombUrl)
			{
				Feed feed = new Feed(url);
				feed.FeedUpdated += (s, e) => Program.RssFeedSendMessage(s, (Feed.UpdatedFeedEventArgs)e);
				_feeds.Add(feed);
			}
			GBTimer = new Timer();
			GBTimer.Elapsed += new ElapsedEventHandler(UpdateFeeds);
			GBTimer.Interval = 1800000;//30 minutes 30*60*1000=1 800 000
			GBTimer.AutoReset = true;
			GBTimer.Enabled = true;
		}

		private void UpdateFeeds(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			foreach (Feed feed in _feeds)
			{
				feed.UpdateFeed();
			}
		}
		public void UpdateFeeds()
		{
			GBTimer.Stop();
			GBTimer.Start();
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
			if (newPubDate.CompareTo(_pubDate) > 0)
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
			return document.XPathSelectElement(@"//rss/channel/item");
		}

		private DateTime getGiantBombFormatDateTime(string dateTimeString)
		{
			string timeString = dateTimeString;
			//This is really ugly, but all the feeds use different ways to encode their timezones and I JUST DONT CARE anymore.
			//Since the feeds are atleast consistent within that particular feed, this shouldn't cause a conflict when comparing timeDates
			timeString = timeString.Replace(" PDT","").Replace(" PST","").Replace(" -0800","");
			return DateTime.ParseExact(timeString,
				"ddd, dd MMM yyyy HH:mm:ss",
				CultureInfo.InvariantCulture);
		}
	}
}
