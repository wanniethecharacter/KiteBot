using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

namespace KiteBot
{
	public class KiteDunk
	{
		private static string[] _kiteDunks;
		private static string[,] _updatedKiteDunks;
		private static CryptoRandom _cryptoRandom;
        private const string GoogleSpreadsheetApiUrl = "https://spreadsheets.google.com/feeds/list/11024r_0u5Mu-dLFd-R9lt8VzOYXWgKX1I5JamHJd8S4/od6/public/values?hl=en_US&&alt=json";
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private static Timer _kiteDunkTimer;

        public KiteDunk()
        {
	        _cryptoRandom = new CryptoRandom();
	        UpdateKiteDunks();
			 
			_kiteDunkTimer = new Timer();
			_kiteDunkTimer.Elapsed += new ElapsedEventHandler(UpdateKiteDunks);
			_kiteDunkTimer.Interval = 86400000;//24 hours
			_kiteDunkTimer.AutoReset = true;
			_kiteDunkTimer.Enabled = true;
        }

		public string GetUpdatedKiteDunk()
		{
			var i = _cryptoRandom.Next(_updatedKiteDunks.GetLength(0));
			return "\"" + _updatedKiteDunks[i, 1] + "\" - " + _updatedKiteDunks[i,0];
		}

		private void UpdateKiteDunks(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			UpdateKiteDunks();
		}

		public void UpdateKiteDunks()
		{
			string response;
			using (var client = new WebClient())
			{
				response = client.DownloadString(GoogleSpreadsheetApiUrl);
			}
			var regex1 = new Regex(@"""gsx\$name"":{""\$t"":""(?<name>[0-9A-Za-z'""., +\-?!\[\]]+?)""},""gsx\$quote"":{""\$t"":""(?<quote>[0-9A-Za-z'""., +\-?!\[\]]+?)""}}", RegexOptions.Singleline);
			var matches = regex1.Matches(response);
			string[,] kiteDunks = new string[matches.Count,2];
			int i = 0;
			foreach (Match match in matches)
			{
				kiteDunks[i, 0] = match.Groups["name"].Value;
				kiteDunks[i++, 1] = match.Groups["quote"].Value;
			}
			_updatedKiteDunks = kiteDunks;
		}
	}
}
