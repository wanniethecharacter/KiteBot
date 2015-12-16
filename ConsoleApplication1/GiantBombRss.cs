using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace KiteBot
{
	class GiantBombRss
	{
		private static readonly string[] GiantBombUrl = { "http://www.giantbomb.com/videos/feed/hd/", "http://www.giantbomb.com/podcast-xml/premium/" };
		private static List<Feed> _feeds;

		public GiantBombRss()
		{
			foreach (string url in GiantBombUrl)
			{
				XDocument document = XDocument.Load(url);
				if (document.Root == null) continue;
				var itemElement = document.Root.Element("rss").Element("item");

				_feeds.Add(new Feed(itemElement, url));
			}
		}
	}

	class Feed
	{
		private XElement _latestXElement;
		private string _url;
		private DateTime _pubDate;

		public Feed(XElement element, string url)
		{
			_latestXElement = element;
			_url = url;
			_pubDate = DateTime.ParseExact(element.Element("pubDate").Value, "ddd, dd MMM yyyy HH:mm:ss PDT", CultureInfo.InvariantCulture);
		}
	}
}
