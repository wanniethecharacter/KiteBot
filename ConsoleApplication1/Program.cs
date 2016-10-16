using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Newtonsoft.Json;
using KiteBot.Commands;
using Game = KiteBot.Commands.Game;


namespace KiteBot
{
    class Program
    {
        static bool _exitSystem = true;

//#region Trap application termination
//        [DllImport("Kernel32")]
//        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

//        private delegate bool EventHandler(CtrlType sig);
//        static EventHandler _handler;

//        enum CtrlType
//        {
//            CtrlCEvent = 0,
//            CtrlBreakEvent = 1,
//            CtrlCloseEvent = 2,
//            CtrlLogoffEvent = 5,
//            CtrlShutdownEvent = 6
//        }

//        private static bool Handler(CtrlType sig)
//        {
//            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

//            //cleanup here
//            KiteChat.MultiDeepMarkovChains.Save();

//            Console.WriteLine("Cleanup complete");

//            //allow main to run off
//            _exitSystem = false;

//            //shutdown right away so there are no lingering threads
//            Environment.Exit(-1);

//            return true;
//        }
//#endregion

        public static DiscordClient Client;
        public static JsonSettings Settings;
        private static KiteChat _kiteChat;
        public static string ContentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        private static string SettingsPath => ContentDirectory + "/Content/settings.json";

        private static void Main()
        {
            //_handler += Handler;
            //SetConsoleCtrlHandler(_handler, true);

            Client = new DiscordClient(x =>
            {
                x.AppName = "KiteBot";
                x.AppVersion = "1.1.1";
                x.MessageCacheSize = 0;
            });

            Settings = File.Exists(SettingsPath) ? 
                JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(SettingsPath)) 
                : new JsonSettings("email",
                "password", 
                "Token", 
                "GBAPIKey", 
                0,
                true, 2, 60000, 60000);

            Client.AddService(new ModuleService());

            Client.UsingCommands(conf =>
            {
                conf.AllowMentionPrefix = true;
                conf.HelpMode = HelpMode.Public;
                conf.PrefixChar = '!';
            });

            _kiteChat = new KiteChat(Settings.MarkovChainStart,
                Settings.GiantBombApiKey,
                Settings.GiantBombLiveStreamRefreshRate,
                Settings.GiantBombVideoRefreshRate, 
                Settings.MarkovChainDepth);            

            Eval.RegisterEvalCommand(Client);
            Game.RegisterGameCommand(Client,Settings.GiantBombApiKey);
            Upcoming.RegisterUpcomingCommand(Client);

            //Event handlers
            Client.UserIsTyping += (s, e) => _kiteChat.IsRaeTyping(e);
            Client.MessageReceived += async (s, e) =>
            {
                try
                {
                    await _kiteChat.AsyncParseChat(s, e, Client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex);
                    Environment.Exit(-1);
                }
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

            Client.UserUpdated += async (s, e) =>
            {
                if (!e.Before.Name.Equals(e.After.Name))
                {
                    await Client.GetChannel(85842104034541568).SendMessage($"{e.Before.Name} changed his name to {e.After.Name}.");
                    _kiteChat.AddWhoIs(e);
                }
                try
                {
                    if (e.Before.Nickname != e.After.Nickname)
                    {
                        if (e.Before.Nickname != null && e.After.Nickname != null)
                        {
                            await
                                Client.GetChannel(85842104034541568)
                                    .SendMessage($"{e.Before.Nickname} changed his nickname to {e.After.Nickname}.");
                            _kiteChat.AddWhoIs(e, e.After.Nickname);
                        }
                        else if (e.Before.Nickname == null && e.After.Nickname != null)
                        {
                            await
                                Client.GetChannel(85842104034541568)
                                    .SendMessage($"{e.Before.Name} set his nickname to {e.After.Nickname}.");
                            _kiteChat.AddWhoIs(e,e.After.Nickname);
                        }
                        else
                        {
                            await
                                Client.GetChannel(85842104034541568)
                                    .SendMessage($"{e.Before.Name} reset his nickname.");
                            _kiteChat.AddWhoIs(e);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex + "\r\n" +ex.Message);
                }
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
                            if (Settings.DiscordEmail == null || Settings.DiscordPassword != null)
                            {
                                await Client.Connect(Settings.DiscordToken,TokenType.Bot);

                            }
                            else
                            {
                                await Client.Connect(Settings.DiscordEmail, Settings.DiscordPassword);
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
                    finally
                    {
                        if (Client.State.CompareTo(ConnectionState.Connected) == 0)
                        {
                            Console.WriteLine("Connected.");
                        }
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
            public ulong OwnerId { get; set; }

            public bool MarkovChainStart { get; set; }
            public int MarkovChainDepth { get; set; }
            public int GiantBombVideoRefreshRate { get; set; }
            public int GiantBombLiveStreamRefreshRate { get; set; }

            public JsonSettings(string email, string password, string token, string gbApi, ulong ownerId, bool markovChainStart,int markovChainDepth, int videoRefresh, int livestreamRefresh)
            {
                DiscordEmail = email;
                DiscordPassword = password;
                DiscordToken = token;
                GiantBombApiKey = gbApi;
                MarkovChainStart = markovChainStart;
                MarkovChainDepth = markovChainDepth;
                GiantBombVideoRefreshRate = videoRefresh;
                GiantBombLiveStreamRefreshRate = livestreamRefresh;
                OwnerId = ownerId;
            }
        }
    }
}
