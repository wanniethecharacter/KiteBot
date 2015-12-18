using System;
using Discord;

namespace KiteBot
{
    class Program
    {
		public static DiscordClient Client;

		static void Main(string[] args)
        {
            Client = new DiscordClient();
			var kiteDunk = new KiteDunk();
			var giantBombRss = new GiantBombRss();

			//Display all log messages in the console
			Client.LogMessage += (s, e) => Console.WriteLine("[{"+e.Severity+"}] {"+e.Source+"}: {"+e.Message+"}");
			
			
			//TODO: Rewrite this as a State Machine
			Client.MessageReceived += async (s, e) =>
			{
				if (!e.Message.IsAuthor && 0 <= e.Message.Text.IndexOf("GetDunked"))
				{
					await Client.SendMessage(e.Channel, "http://i.imgur.com/QhcNUWo.gifv");
				}
				if (!e.Message.IsAuthor && e.Message.Text.StartsWith("@KiteBot"))
				{
					if (e.Message.Text.StartsWith("@KiteBot #420") || e.Message.Text.ToLower().StartsWith("@KiteBot #blaze") ||
					    0 <= e.Message.Text.ToLower().IndexOf("waifu", 0))
					{
						await Client.SendMessage(e.Channel, "http://420.moe/");
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("help", 5))
					{
						var nl = Environment.NewLine;
						await Client.SendMessage(e.Channel, "Current Commands are:" + nl + "#420" + nl + "google" + nl + "youtube" + nl + "kitedunk" + nl + "help");
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("google", 0))
					{
						await Client.SendMessage(e.Channel, "http://lmgtfy.com/?q=" + e.Message.Text.ToLower().Substring(16).Replace(' ', '+'));
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("youtube", 0) && e.Message.Text.Length > 16)
					{
						await Client.SendMessage(e.Channel, "https://www.youtube.com/results?search_query=" + e.Message.Text.ToLower().Substring(17).Replace(' ', '+'));
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("dunk", 0))
					{
						await Client.SendMessage(e.Channel, kiteDunk.GetRandomKiteDunk());
					}
					else
					{
						await Client.SendMessage(e.Channel, "KiteBot ver. 0.5-PreAlpha \"This one is for the ladies.\"");
					}
				}
			};

			//Convert our sync method to an async one and block the Main function until the bot disconnects
			Client.Run(async () =>
			{
				//Connect to the Discord server using our email and password
				await Client.Connect("", "");
			});
        }
	    public static void SendMessage(string message)
	    {
			//ToDO make this server generic
		    Client.SendMessage(Client.GetChannel(85842104034541568),message);
	    }

	    public static void SendMessage(object s, Feed.UpdatedFeedEventArgs e)
	    {
		    Client.SendMessage(Client.GetChannel(85842104034541568),
			    e.Title + " live now at GiantBomb.com\r\n" + e.Link);
	    }
    }
}
