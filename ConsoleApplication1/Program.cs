using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.Http;
using System.Text.RegularExpressions;


namespace ConsoleApplication1
{
    class Program
    {
		public static Discord.DiscordClient client;
		public static long id = 85842104034541568;
		public static string kiteDunksApi = "https://spreadsheets.google.com/feeds/list/11024r_0u5Mu-dLFd-R9lt8VzOYXWgKX1I5JamHJd8S4/od6/public/values?hl=en_US&&alt=json";
		public static string[] kiteDunks;
		public static Random rnd;

		static void Main(string[] args)
        {
            client = new Discord.DiscordClient();
			GetKiteDunksAsync();
			rnd = new Random(System.DateTime.Now.Millisecond + System.DateTime.Now.Minute);

			//Display all log messages in the console
			client.LogMessage += (s, e) => Console.WriteLine("[{"+e.Severity+"}] {"+e.Source+"}: {"+e.Message+"}");

			//Echo back any message received, provided it didn't come from the bot itself
			client.MessageReceived += async (s, e) =>
			{
				if (!e.Message.IsAuthor && 0 <= e.Message.Text.IndexOf("GetDunked "))
				{
					await client.SendMessage(e.Channel, "http://i.imgur.com/QhcNUWo.gifv");
				}
				if (!e.Message.IsAuthor && e.Message.Text.StartsWith("@KiteBot"))
				{
					if (e.Message.Text.StartsWith("@KiteBot #420") || e.Message.Text.ToLower().StartsWith("@KiteBot #blaze") || 0 <= e.Message.Text.ToLower().IndexOf("waifu",0))
					{
						await client.SendMessage(e.Channel, "http://420.moe/");
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("help", 5))
					{
						var nl = System.Environment.NewLine;
						await client.SendMessage(e.Channel, "Current Commands are:"+ nl +"#420"+ nl + "google"+ nl + "youtube" + nl + "kitedunk" + nl + "help");
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("google", 0)) 
					{
						await client.SendMessage(e.Channel, "http://lmgtfy.com/?q=" + e.Message.Text.ToLower().Substring(16).Replace(' ','+'));
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("youtube", 0))
					{
						await client.SendMessage(e.Channel, "https://www.youtube.com/results?search_query=" + e.Message.Text.ToLower().Substring(17).Replace(' ','+'));
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("dunk", 0))
					{
						await client.SendMessage(e.Channel, KiteDunk());
					}
					else
					{
						await client.SendMessage(e.Channel, "KiteBot ver. 0.2a-PreAlpha \" MOKE WEED EVRY DAY\"");
					}					
				}
			};

			//Convert our sync method to an async one and block the Main function until the bot disconnects
			client.Run(async () =>
			{
				//Connect to the Discord server using our email and password
				await client.Connect(KiteBot.Properties.auth.Default.email, KiteBot.Properties.auth.Default.password);
			});
        }
		static void GetKiteDunksAsync() 
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\Users\sindr\Documents\visual studio 2013\Projects\ConsoleApplication1\ConsoleApplication1\KiteDunks2.txt");
			kiteDunks = lines;
		}

		private static string KiteDunk() {
			int i = rnd.Next(kiteDunks.Length/2)*2;
			string s = "\"" + kiteDunks[i+1] + "\" - " + kiteDunks[i];
			return s;
		}
		//Not currently used
		private static string GetPlainTextFromHtml(string htmlString)
		{
			string htmlTagPattern = "<.*?>";
			var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
			htmlString = regexCss.Replace(htmlString, string.Empty);
			htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
			htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
			htmlString = htmlString.Replace("&nbsp;", string.Empty);

			return htmlString;
		}
    }
}
