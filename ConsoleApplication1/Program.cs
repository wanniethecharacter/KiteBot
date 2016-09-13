using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Modules;
using Newtonsoft.Json;
using KiteBot.Commands;


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

        public static DiscordSocketClient Client;
        public static CommandService CommandService;
        public static JsonSettings Settings;
        private static KiteChat _kiteChat;
        public static string ContentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        private static string SettingsPath => ContentDirectory + "/Content/settings.json";

        private static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            //_handler += Handler;
            //SetConsoleCtrlHandler(_handler, true);
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 0                
            });

            Settings = File.Exists(SettingsPath) ? 
                JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(SettingsPath)) 
                : new JsonSettings("email",
                "password", 
                "Token", 
                "GBAPIKey", 
                0,
                true, 2, 60000, 60000);

            _kiteChat = new KiteChat(Settings.MarkovChainStart,
                Settings.GiantBombApiKey,
                Settings.GiantBombLiveStreamRefreshRate,
                Settings.GiantBombVideoRefreshRate, 
                Settings.MarkovChainDepth);            

            //Event handlers
            Client.UserIsTyping += (u, c) =>
            {
                _kiteChat.IsRaeTyping(u);
                return Task.CompletedTask;
            };

            Client.MessageReceived += async (msg) =>
            {
                try
                {
                    await _kiteChat.AsyncParseChat(msg, Client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex);
                    Environment.Exit(-1);
                }
            }; 

            Client.GuildAvailable += async (server) =>
            {
                if (Client.GetGuilds().Any())
                {
                    Console.WriteLine(await _kiteChat.InitializeMarkovChain());
                }
            };
            
            Client.JoinedGuild += (server) =>
            {
                Console.WriteLine("Connected to " + server.Name);
                return Task.CompletedTask;
            };

            Client.UserUpdated += async (before, after) =>
            {
                var channel = (ITextChannel)Client.GetChannel(85842104034541568);
                if (!before.Username.Equals(after.Username))
                {                    
                    await channel.SendMessageAsync($"{before.Username} changed his name to {after.Username}.");
                    _kiteChat.AddWhoIs(before, after);
                }
                try
                {
                    if (before.Nickname != after.Nickname)
                    {
                        if (before.Nickname != null && after.Nickname != null)
                        {
                            await channel.SendMessageAsync($"{before.Nickname} changed his nickname to {after.Nickname}.");
                            _kiteChat.AddWhoIs(before, after.Nickname);
                        }
                        else if (before.Nickname == null && after.Nickname != null)
                        {
                            await channel.SendMessageAsync($"{before.Username} set his nickname to {after.Nickname}.");
                            _kiteChat.AddWhoIs(before, after.Nickname);
                        }
                        else
                        {
                            await channel.SendMessageAsync($"{before.Username} reset his nickname.");
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex + "\r\n" + ex.Message);
                }
            };

            await InstallCommands();

            await Client.LoginAsync(TokenType.Bot, Settings.DiscordToken);
            // Connect the client to Discord's gateway
            await Client.ConnectAsync();
            
            await Task.Delay(-1);
        }
        [Obsolete]
        public static void RssFeedSendMessage(object s, Feed.UpdatedFeedEventArgs e)
        {
            var channel = (ITextChannel)Client.GetChannel(85842104034541568);

            channel.SendMessageAsync(e.Title + " live now at GiantBomb.com\r\n" + e.Link);
	    }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            Client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await CommandService.LoadAssembly(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(IMessage paramMessage)
        {
            // Cast paramMessage to an IUserMessage, return if the message was a System message.
            var msg = paramMessage as IUserMessage;
            if (msg == null) return;
            // Internal integer, marks where the command begins
            int argPos = 0;
            // Get the current user (used for Mention parsing)
            var currentUser = await Client.GetCurrentUserAsync();
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (msg.HasCharPrefix('~', ref argPos) || msg.HasMentionPrefix(currentUser, ref argPos))
            {
                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed succesfully)
                var result = await CommandService.Execute(msg, argPos);
                if (!result.IsSuccess)
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
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
