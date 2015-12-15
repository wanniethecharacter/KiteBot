using System;

namespace KiteBot
{
    class Program
    {
		public static Discord.DiscordClient Client;

		static void Main(string[] args)
        {
            Client = new Discord.DiscordClient();
			var kiteDunk = new KiteBot.KiteDunk();

			//Display all log messages in the console
			Client.LogMessage += (s, e) => Console.WriteLine("[{"+e.Severity+"}] {"+e.Source+"}: {"+e.Message+"}");
			
			
			//Echo back any message received, provided it didn't come from the bot itself
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
					else if (0 <= e.Message.Text.ToLower().IndexOf("youtube", 0))
					{
						await Client.SendMessage(e.Channel, "https://www.youtube.com/results?search_query=" + e.Message.Text.ToLower().Substring(17).Replace(' ', '+'));
					}
					else if (0 <= e.Message.Text.ToLower().IndexOf("dunk", 0))
					{
						await Client.SendMessage(e.Channel, kiteDunk.GetRandomKiteDunk());
					}
					else
					{
						await Client.SendMessage(e.Channel, "KiteBot ver. 0.4-PreAlpha \"Fuck you\"");
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
    }
}
