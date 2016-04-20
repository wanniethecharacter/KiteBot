using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace KiteCore
{
	public class KiteDunk
	{
		private static string[,] _updatedKiteDunks;
	    private static Random _random;
		//private static CryptoRandom _cryptoRandom;
        private const string GoogleSpreadsheetApiUrl = "https://spreadsheets.google.com/feeds/list/11024r_0u5Mu-dLFd-R9lt8VzOYXWgKX1I5JamHJd8S4/od6/public/values?hl=en_US&&alt=json";
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private static Timer _kiteDunkTimer;

        public KiteDunk()
        {
	        //_cryptoRandom = new CryptoRandom();
            _random = new Random();
	        UpdateKiteDunks().Wait();

			_kiteDunkTimer = new Timer(async delegate {await UpdateKiteDunks();}, null, 0, 120000);
        }

		public string GetUpdatedKiteDunk()
		{
			var i = _random.Next(_updatedKiteDunks.GetLength(0));
			return "\"" + _updatedKiteDunks[i, 1] + "\" - " + _updatedKiteDunks[i, 0];
		}

        public async Task UpdateKiteDunks()
        {
            try
            {
                string response;
                using (var client = new HttpClient())
                {
                    response = await client.GetStringAsync(GoogleSpreadsheetApiUrl);
                }
                var regex1 =
                    new Regex(
                        @"""gsx\$name"":{""\$t"":""(?<name>[0-9A-Za-z'""., +\-?!\[\]]+?)""},""gsx\$quote"":{""\$t"":""(?<quote>[0-9A-Za-z'""., +\-?!\[\]]+?)""}}",
                        RegexOptions.Singleline);
                var matches = regex1.Matches(response);
                string[,] kiteDunks = new string[matches.Count, 2];
                int i = 0;
                foreach (Match match in matches)
                {
                    kiteDunks[i, 0] = match.Groups["name"].Value;
                    kiteDunks[i++, 1] = match.Groups["quote"].Value;
                }
                _updatedKiteDunks = kiteDunks;
            }
            catch (Exception e)
            {
                Console.WriteLine("Update of KiteDunks failed, retrying... "+ e.Message);
                await Task.Delay(5000);
                await UpdateKiteDunks();
            }
        }
    }
}
