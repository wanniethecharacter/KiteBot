using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace KiteBot
{
    class Program
    {
        static bool _exitSystem = true;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CtrlCEvent = 0,
            CtrlBreakEvent = 1,
            CtrlCloseEvent = 2,
            CtrlLogoffEvent = 5,
            CtrlShutdownEvent = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            //cleanup here
            KiteChat.MultiDeepMarkovChains.Save();

            Console.WriteLine("Cleanup complete");

            //allow main to run off
            _exitSystem = false;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion

        public static DiscordClient Client;
        private static JsonSettings _settings;
        private static KiteChat _kiteChat;
        public static string ContentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        private static string SettingsPath => ContentDirectory + "\\Content\\settings.json";

        private static void Main()
        {
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

            Client = new DiscordClient();
            _settings = File.Exists(SettingsPath) ? 
                JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(SettingsPath)) 
                : new JsonSettings("email", "password", 
                "Token", 
                "GBAPIKey", 
                true, 2, 60000, 60000);

            _kiteChat = new KiteChat(_settings.MarkovChainStart,
                _settings.GiantBombApiKey,
                _settings.GiantBombLiveStreamRefreshRate,
                _settings.GiantBombVideoRefreshRate, 
                _settings.MarkovChainDepth);

            //Event handlers
            Client.UserIsTyping += async (s, e) => await Task.Run(delegate { _kiteChat.IsRaeTyping(e); });

            Client.MessageReceived += async (s, e) =>
            {
                await _kiteChat.AsyncParseChat(s, e, Client);
            }; 

            Client.ServerAvailable += async (s, e) =>
            {
                if (Client.Servers.Any())
                {
                    Console.WriteLine( await _kiteChat.InitializeMarkovChain());
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
                            if (_settings.DiscordEmail == null || _settings.DiscordPassword != null)
                            {
                                await Client.Connect(_settings.DiscordToken);

                            }
                            else
                            {
                                await Client.Connect(_settings.DiscordEmail,_settings.DiscordPassword);
                            }
                            
                            Client.SetGame("with Fury v1.1.0");
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

        public static void RssFeedSendMessage(object s, Feed.UpdatedFeedEventArgs e)
	    {
		    Client.GetChannel(85842104034541568).SendMessage(e.Title + " live now at GiantBomb.com\r\n" + e.Link);
	    }

        public struct JsonSettings
        {
            public string DiscordEmail { get; set; }
            public string DiscordPassword { get; set; }
            public string DiscordToken { get; set; }
            public string GiantBombApiKey { get; set; }

            public bool MarkovChainStart { get; set; }
            public int MarkovChainDepth { get; set; }
            public int GiantBombVideoRefreshRate { get; set; }
            public int GiantBombLiveStreamRefreshRate { get; set; }

            public JsonSettings(string email, string password, string token, string gbApi, bool markovChainStart,int markovChainDepth, int videoRefresh, int livestreamRefresh)
            {
                DiscordEmail = email;
                DiscordPassword = password;
                DiscordToken = token;
                GiantBombApiKey = gbApi;
                MarkovChainStart = markovChainStart;
                MarkovChainDepth = markovChainDepth;
                GiantBombVideoRefreshRate = videoRefresh;
                GiantBombLiveStreamRefreshRate = livestreamRefresh;
            }
        }
    }
}
