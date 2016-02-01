using System;
using Discord;

namespace KiteBot
{
    class Program
    {
		public static DiscordClient Client;

        private static void Main(string[] args)
	    {
		    Client = new DiscordClient();
		    var kiteChat = new KiteChat();
            //bool shutUp = false;
            //Display all log messages in the console
            Client.LogMessage += (s, e) => Console.WriteLine("[{"+e.Severity+"}] {"+e.Source+"}: {"+e.Message+"}");

	        Client.UserIsTypingUpdated += (s, e) => kiteChat.IsRaeTyping(e);

			Client.MessageReceived += async (s, e) => await kiteChat.AsyncParseChat(s, e, Client);

            Client.Connected += async (s, e) => Console.WriteLine(await KiteChat.TextMarkovChainHelper.Initialize());

			//Convert our sync method to an async one and block the Main function until the bot disconnects
		    Client.Run(async () =>
			{
				//Connect to the Discord server using our email and password
				await Client.Connect(Properties.auth.Default.DiscordEmail,Properties.auth.Default.DiscordPassword);
			});
        }

	    public static void RssFeedSendMessage(object s, Feed.UpdatedFeedEventArgs e)
	    {
		    Client.SendMessage(Client.GetChannel(85842104034541568),
			    e.Title + " live now at GiantBomb.com\r\n" + e.Link);
	    }
    }
}
