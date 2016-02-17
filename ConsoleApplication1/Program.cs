using System;
using KiteBot.Properties;
using Discord;

namespace KiteBot
{
    class Program
    {
		public static DiscordClient Client;
        private static KiteChat kiteChat;

        private static void Main(string[] args)
        {
            Client = new DiscordClient();
            kiteChat = new KiteChat();

            //Display all log messages in the console
            //Client.LogMessage += (s, e) => Console.WriteLine("[{"+e.Severity+"}] {"+e.Source+"}: {"+e.Message+"}");

	        Client.UserIsTyping += (s, e) => kiteChat.IsRaeTyping(e);

			Client.MessageReceived += async (s, e) => await kiteChat.AsyncParseChat(s, e, Client);

            Client.LoggedIn += async (s, e) =>
            {
                Console.WriteLine(await KiteChat.MultiDeepMarkovChain.Initialize());
            };

			//Convert our sync method to an async one and block the Main function until the bot disconnects
		    Client.ExecuteAndWait(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Client.Connect(auth.Default.DiscordEmail, auth.Default.DiscordPassword);
#if DEBUG
                        Client.SetGame("with Fire");
#else
                        Client.SetGame("with Freedom");
#endif

                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("FUCK THIS :" + ex.Message);
                    }
                }
            });
        }

	    public static void RssFeedSendMessage(object s, Feed.UpdatedFeedEventArgs e)
	    {
		    Client.GetChannel(85842104034541568).SendMessage(e.Title + " live now at GiantBomb.com\r\n" + e.Link);
	    }
    }
}
