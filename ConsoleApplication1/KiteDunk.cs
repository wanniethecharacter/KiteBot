using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace KiteBot
{
	public class KiteDunk
	{
		public static string[] _kiteDunks;
		public static Random _randomSeed;
        public const string FileLocation = @"C:\Users\sindr\Documents\visual studio 2013\Projects\ConsoleApplication1\ConsoleApplication1\KiteDunks3.txt";
        public const string GoogleSpreadsheetApiUrl = "https://spreadsheets.google.com/feeds/list/11024r_0u5Mu-dLFd-R9lt8VzOYXWgKX1I5JamHJd8S4/od6/public/values?hl=en_US&&alt=json";

        public KiteDunk() : this(File.ReadAllLines(FileLocation), new Random(DateTime.Now.Millisecond))
        {
        }

        public KiteDunk(string[] arrayOfDunks, Random randomSeed)
        {
            _kiteDunks = arrayOfDunks;
            _randomSeed = randomSeed;
        }

        /// <summary>
        ///     Returns a Message from Random Seed
        /// </summary>
        /// <returns>New Message from String</returns>
		public string GetRandomKiteDunk()
		{
            //TODO: Maybe make this a retry recursion method
			var i = _randomSeed.Next(_kiteDunks.Length / 2) * 2;
			return "\"" + _kiteDunks[i + 1] + "\" - " + _kiteDunks[i];
		}

		private void UpdateKiteDunks()
		{
			string response;
			using (var client = new WebClient())
			{
				response = client.DownloadString("");
			}
			var regex1 = new Regex("\"gsx\\$name\":{\"\\$t\":\".+?\"}|\"gsx\\$quote\":{\"\\$t\":\".+?\"}}", RegexOptions.Singleline);
			var matches = regex1.Matches(response);
			var i = 0;
			var kiteDunks = new string[matches.Count];
			foreach(var match in matches)
			{
				var s = match.ToString().Replace("\"gsx$name\":{\"$t\":","").Replace("gsx$quote\":{\"$t\":","").Replace("}","").Replace("\"","");
				kiteDunks[i] = s;
				i++;
			}
			File.WriteAllLines(path: "C:\\Users\\sindr\\Documents\\visual studio 2013\\Projects\\ConsoleApplication1\\ConsoleApplication1\\KiteDunks2.txt", contents: kiteDunks);
		}
	}
}
