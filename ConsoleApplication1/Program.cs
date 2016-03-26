using System;
using System.Runtime.InteropServices;
using KiteBot.Properties;
using Discord;

namespace KiteBot
{
    class Program
    {
        static bool exitSystem;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            //cleanup here
            KiteChat.MultiDeepMarkovChain.Save();

            Console.WriteLine("Cleanup complete");

            //allow main to run off
            exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion

        public static DiscordClient Client;
        private static KiteChat kiteChat;

        private static void Main(string[] args)
        {
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

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
                while (!exitSystem)
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
