using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

namespace KiteBot
{
	public class KiteDunk
	{
		private static string[] _kiteDunks;
		private static string[,] _updatedKiteDunks;
		private static Random _randomSeed;
		private static CryptoRandom _cryptoRandom;
        private static readonly string DunkDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        private static readonly string FileLocation = DunkDirectory + "\\Content\\KiteDunks2.txt";
        private const string GoogleSpreadsheetApiUrl = "https://spreadsheets.google.com/feeds/list/11024r_0u5Mu-dLFd-R9lt8VzOYXWgKX1I5JamHJd8S4/od6/public/values?hl=en_US&&alt=json";
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private static Timer _kiteDunkTimer;

        public KiteDunk() : this(File.ReadAllLines(FileLocation), new Random(DateTime.Now.Millisecond), new CryptoRandom())
        {
        }

        public KiteDunk(string[] arrayOfDunks, Random randomSeed, CryptoRandom cryptoRandom)
        {
            _kiteDunks = arrayOfDunks;
            _randomSeed = randomSeed;
	        _cryptoRandom = cryptoRandom;
	        UpdateKiteDunks();
			 
			_kiteDunkTimer = new Timer();
			_kiteDunkTimer.Elapsed += new ElapsedEventHandler(UpdateKiteDunks);
			_kiteDunkTimer.Interval = 86400000;//24 hours
			_kiteDunkTimer.AutoReset = true;
			_kiteDunkTimer.Enabled = true;
        }

		[Obsolete]
		public string GetRandomKiteDunk()
		{
            //TODO: Maybe make this a retry recursion method
			var i = _randomSeed.Next(_kiteDunks.Length / 2) * 2;
			return "\"" + _kiteDunks[i + 1] + "\" - " + _kiteDunks[i];
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

			/* var i = 0;
			var kiteDunks = new string[matches.Count];
			foreach(var match in matches)
			{
				var s = match.ToString().Replace("\"gsx$name\":{\"$t\":","").Replace("gsx$quote\":{\"$t\":","").Replace("}","").Replace("\"","");
				kiteDunks[i] = s;
				i++;
			}
			File.WriteAllLines(path: DunkDirectory + "\\Content\\KiteDunks3.txt", contents: kiteDunks); */
		}
	}
}
