using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

namespace KiteBot
{
	public class KiteDunk
	{
		public static string[] _kiteDunks;
		public static string[,] UpdatedKiteDunks;
		public static Random _randomSeed;
        public static string DunkDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        public static string FileLocation = DunkDirectory + "\\Content\\KiteDunks2.txt";
        public const string GoogleSpreadsheetApiUrl = "https://spreadsheets.google.com/feeds/list/11024r_0u5Mu-dLFd-R9lt8VzOYXWgKX1I5JamHJd8S4/od6/public/values?hl=en_US&&alt=json";
		private static Timer KiteDunkTimer;

        public KiteDunk() : this(File.ReadAllLines(FileLocation), new Random(DateTime.Now.Millisecond))
        {
        }

        public KiteDunk(string[] arrayOfDunks, Random randomSeed)
        {
            _kiteDunks = arrayOfDunks;
            _randomSeed = randomSeed;
	        UpdateKiteDunks();

			KiteDunkTimer = new Timer();
			KiteDunkTimer.Elapsed += new ElapsedEventHandler(UpdateKiteDunks);
			KiteDunkTimer.Interval = 86400000;//24 hours
			KiteDunkTimer.AutoReset = true;
			KiteDunkTimer.Enabled = true;
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
			var i = _randomSeed.Next(UpdatedKiteDunks.GetLength(0));
			return "\"" + UpdatedKiteDunks[i, 1] + "\" - " + UpdatedKiteDunks[i,0];
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
			var regex1 = new Regex(@"""gsx\$name"":{""\$t"":""(?<name>[0-9A-Za-z'""., +\-?!@\[\]]+?)""},""gsx\$quote"":{""\$t"":""(?<quote>[0-9A-Za-z'""., +\-?!@\[\]]+?)""}}", RegexOptions.Singleline);
			var matches = regex1.Matches(response);
			string[,] kiteDunks = new string[matches.Count,2];
			int i = 0;
			foreach (Match match in matches)
			{
				kiteDunks[i, 0] = match.Groups["name"].Value;
				kiteDunks[i++, 1] = match.Groups["quote"].Value;
			}
			UpdatedKiteDunks = kiteDunks;

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
