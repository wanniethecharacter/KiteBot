using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KiteBot
{
	class KiteDunk
	{
		private static string kiteDunksApi = "https://spreadsheets.google.com/feeds/list/11024r_0u5Mu-dLFd-R9lt8VzOYXWgKX1I5JamHJd8S4/od6/public/values?hl=en_US&&alt=json";
		private string[] kiteDunks;
		
		KiteDunk()
		{
			string response;
			using (WebClient client = new WebClient())
			{
				response = client.DownloadString(kiteDunksApi);
			}
			dynamic o = JsonConvert.DeserializeObject(response);
			o.
		}
	}
}
