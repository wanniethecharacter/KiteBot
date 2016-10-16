using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using KiteBot.Json;
using Newtonsoft.Json;


namespace KiteBot
{
    class Program
    {
        public static DiscordSocketClient Client;
        public static CommandService CommandService = new CommandService();
        public static BotSettings Settings;
        private static KiteChat _kiteChat;
        public static string ContentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        private static string SettingsPath => ContentDirectory + "/Content/settings.json";
        private static CommandHandler _handler;

        private static void Main(string[] args) => AsyncMain(args).GetAwaiter().GetResult();

        public static async Task AsyncMain(string[] args)
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 0
            });

            Settings = File.Exists(SettingsPath) ? 
                JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(SettingsPath)) 
                : new BotSettings()
                {
                    DiscordEmail = "email",
                    DiscordPassword = "password",
                    DiscordToken = "Token",
                    GiantBombApiKey = "GBAPIKey",
                    OwnerId = 0,
                    MarkovChainStart = false,
                    MarkovChainDepth = 2,
                    GiantBombLiveStreamRefreshRate = 60000, GiantBombVideoRefreshRate = 60000
                };

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

            Client.MessageReceived += async msg =>
            {
                Console.WriteLine(msg.ToString());
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

            Client.GuildAvailable += async server =>
            {
                if (Client.Guilds.Any())
                {
                    Console.WriteLine(await _kiteChat.InitializeMarkovChain());
                }
            };
            
            Client.JoinedGuild += server =>
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

            await Client.LoginAsync(TokenType.Bot, Settings.DiscordToken);
            await Client.ConnectAsync();

            var map = new DependencyMap();
            map.Add(Client);

            _handler = new CommandHandler();
            await _handler.Install(map);

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
