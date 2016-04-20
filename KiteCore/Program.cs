using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;

namespace KiteCore
{
    class Program
    {
        private static bool _exitSystem = true;

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
            _exitSystem = false;

            //shutdown right away so there are no lingering threads
            Environment.FailFast("Shutting down, hold on...");

            return true;
        }
        #endregion

        public static DiscordClient Client;
        private static KiteChat _kiteChat;

        private static void Main()
        {
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

            Client = new DiscordClient();
            _kiteChat = new KiteChat();

            //Event handlers
            Client.UserIsTyping += (s, e) => _kiteChat.IsRaeTyping(e);

            Client.MessageReceived += async (s, e) =>
            {
                await _kiteChat.AsyncParseChat(s, e, Client);
            };

            Client.ServerAvailable += async (s, e) =>
            {
                if (Client.Servers.Any())
                {
                    Console.WriteLine(await _kiteChat.InitializeMarkovChain());
                }
            };

            Client.JoinedServer += (s, e) =>
            {
                Console.WriteLine("Connected to " + e.Server.Name);
            };

            //Convert our sync method to an async one and block the Main function until the bot disconnects
            Client.ExecuteAndWait(async () =>
            {
                while (_exitSystem)
                {
                    try
                    {
                        if (Client.State.CompareTo(ConnectionState.Disconnected) == 0)
                        {
                            Console.WriteLine("Connecting...");
                            //await Client.Connect(auth.Default.DiscordEmail, auth.Default.DiscordPassword);
                            await Client.Connect("***REMOVED***");
                            Client.SetGame("with your donger");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Login Failed: " + ex.Message);
                        await Task.Delay(5000);
                    }
                }
            });
        }
    }
}
