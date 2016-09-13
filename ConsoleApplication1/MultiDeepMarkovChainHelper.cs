using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.API;
using Discord.API.Rest;
using Newtonsoft.Json;
using TextMarkovChains;

namespace KiteBot
{
    public class MultiTextMarkovChainHelper
    {
        public static int Depth;
        private static Timer _timer;
        private static IMarkovChain _markovChain;
        private static IDiscordClient _client;
        private static bool _isInitialized;
        private static List<JsonMessage> _jsonList = new List<JsonMessage>();
        private static JsonLastMessage _lastMessage = new JsonLastMessage();

        public static string RootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        public static string JsonLastMessageLocation => RootDirectory + "/Content/LastMessage.json";
        public static string JsonMessageFileLocation => RootDirectory + "/Content/messages.zip";
        

        public MultiTextMarkovChainHelper(int depth) : this(Program.Client, depth)
        {
        }

        public MultiTextMarkovChainHelper(IDiscordClient client,int depth)
        {
            Console.WriteLine("MultiTextMarkovChainHelper");
            _client = client;
            Depth = depth;
            switch (depth)
            {
                case 1:
                    _markovChain = new TextMarkovChain();
                    break;
                case 2:
                    _markovChain = new DeepMarkovChain();
                    break;
                default:
                    if (depth < 2)
                    {
                        _markovChain = new MultiDeepMarkovChain(Depth);
                    }
                    else
                    {
                        _markovChain = new TextMarkovChain();
                    }
                    break;
            }

            _timer = new Timer();
            _timer.Elapsed += async (s,e) => await Save();
            _timer.Interval = 3600000;
            _timer.AutoReset = true;
        }

        public async Task<bool> Initialize()
        {
            Console.WriteLine("Initialize");
            if (!_isInitialized)
            {
                if (File.Exists(path: JsonMessageFileLocation))
                {
                    _jsonList = JsonConvert.DeserializeObject<List<JsonMessage>>(Open());

                    foreach (JsonMessage message in _jsonList)
                    {
                        //if(GC.GetTotalMemory(false) < 512000000)
                            _markovChain.feed(message.M);//Any messages here have already been thru all the if checks, so we dont need to run through all of those again.
                    }
                    _isInitialized = true;
                    if (File.Exists(JsonLastMessageLocation))
                    {
                        try
                        {
                            string s = File.ReadAllText(JsonLastMessageLocation);
                            _lastMessage = JsonConvert.DeserializeObject<JsonLastMessage>(s);
                            List<Message> list = await DownloadMessagesAfterId(_lastMessage.MessageId, _lastMessage.ChannelId);
                            foreach (Message message in list)
                            {
                                FeedMarkovChain(message);
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("fucking Last Message JSON is killing me");
                        }
                    }
                }
                else
                {
                    List<Message> list = await GetMessagesFromChannel(85842104034541568, 10000);
                    list.AddRange(await GetMessagesFromChannel(96786127238725632, 5000));
                    list.AddRange(await GetMessagesFromChannel(94122326802571264, 5000));
                    foreach (Message message in list)
                    {
                        if (message != null && !message.Content.Equals(""))
                        {
                            FeedMarkovChain(message);
                            var json = new JsonMessage{M = message.Content.Value};
                            _jsonList.Add(json);
                        }
                    }
                }
                _timer.Enabled = true;
                return _isInitialized = true;
            }
            return _isInitialized;
        }

        private async Task<List<Message>> DownloadMessagesAfterId(ulong id, ulong channelId)
        {
            Console.WriteLine("DownloadMessagesAfterId");
            List<Message> messages = new List<Message>();
            var latestMessages = await _client.ApiClient.GetChannelMessagesAsync(channelId, new GetChannelMessagesParams { Limit = 100, RelativeDirection = Direction.After });
            messages.AddRange(latestMessages);

            if (latestMessages.Count == 0) return messages;

            ulong tmpMessageTracker = latestMessages.Last().Id;
            // ReSharper disable once TooWideLocalVariableScope
            ulong newMessageTracker;

            while (true)
            {
                messages.AddRange(await _client.ApiClient.GetChannelMessagesAsync(channelId, new GetChannelMessagesParams { Limit = 100, RelativeMessageId = tmpMessageTracker, RelativeDirection = Direction.After}));

                newMessageTracker = messages[messages.Count - 1].Id;
                if (tmpMessageTracker != newMessageTracker)     //Checks if there are any more messages in channel, and if not, returns the List
                {
                    tmpMessageTracker = newMessageTracker;      //grabs the last message added, and uses the new Id as the start of the next query
                }
                else
                {
                    messages.RemoveAt(messages.Count - 1);      //removes the excessive object
                    return messages;
                }
            }
        }

        public void Feed(IMessage message)
        {
            FeedMarkovChain(message);
        }

        public string GetSequence()
        {
            if (_isInitialized)
            {
                try
                {
                    return _markovChain.generateSentence();
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("Nullref fun "+ex.Message);
                    return GetSequence();
                }
            }
            return "I'm not ready yet Senpai!";
        }

        private void FeedMarkovChain(IMessage message)
        {
            if (!message.Author.IsBot)
            {
                if(!message.Content.Equals("") && !message.Content.Contains("http") && !message.Content.ToLower().Contains("testmarkov") && !message.Content.ToLower().Contains("getdunked"))//TODO: add back in is mentioning me check
                {
                    if (message.Content.Contains("."))
                    {
                        if (GC.GetTotalMemory(false) < 512000000)
                            _markovChain.feed(message.Content);
                    }
                    _markovChain.feed(message.Content + ".");
                    _jsonList.Add(new JsonMessage { M = message.Content });
                }
            }
        }

        private void FeedMarkovChain(Message message)
        {
            if (message.Author.IsSpecified && !message.Author.Value.Bot.Value)
            {
                if (!message.Content.Value.Equals("") && !message.Content.Value.Contains("http") && !message.Content.Value.ToLower().Contains("testmarkov") && !message.Content.Value.ToLower().Contains("getdunked") && message.Mentions.Value.First().Username.Value == "KiteBot")//TODO: add back in is mentioning me check
                {
                    if (message.Content.Value.Contains("."))
                    {
                        if (GC.GetTotalMemory(false) < 512000000)
                            _markovChain.feed(message.Content.Value);
                    }
                    _markovChain.feed(message.Content + ".");
                    _jsonList.Add(new JsonMessage { M = message.Content.Value });
                }
            }
        }

        private async Task<List<Message>> GetMessagesFromChannel(ulong channelId, int i)
        {
            Console.WriteLine("GetMessagesFromChannel");
            List<Message> messages = new List<Message>();
            var latestMessage = await _client.ApiClient.GetChannelMessagesAsync(channelId, new GetChannelMessagesParams{Limit = 100});
            messages.AddRange(latestMessage);

            ulong tmpMessageTracker = latestMessage.Last().Id;

            while (messages.Count < i)
            {
                messages.AddRange(await _client.ApiClient.GetChannelMessagesAsync(channelId, new GetChannelMessagesParams { Limit = 100, RelativeMessageId = tmpMessageTracker, RelativeDirection = Direction.Before}));

                ulong newMessageTracker = messages[messages.Count - 1].Id;
                if (tmpMessageTracker != newMessageTracker)     //Checks if there are any more messages in channel, and if not, returns the List
                {
                    tmpMessageTracker = newMessageTracker;      //grabs the last message added, and uses the new Id as the start of the next query
                }
                else
                {
                    messages.RemoveAt(messages.Count - 1);      //removes the excessive object
                    return messages;
                }
            }
            return messages;
        }

        public async Task Save()
        {
            Console.WriteLine("Save");
            if (_isInitialized)
            {
                var text = Encoding.Default.GetBytes(JsonConvert.SerializeObject(_jsonList, Formatting.None));
                using (var fileStream = File.Open(JsonMessageFileLocation, FileMode.OpenOrCreate))
                {
                    using (var stream = new GZipStream(fileStream, CompressionLevel.Optimal))
                    {
                        stream.Write(text, 0, text.Length);     // Write to the `stream` here and the result will be compressed
                    }
                }
                var message = await _client.ApiClient.GetChannelMessagesAsync(85842104034541568, new GetChannelMessagesParams {Limit = 1});
                var x = new JsonLastMessage
                {
                    MessageId = message.First().Id,
                    ChannelId = message.First().ChannelId
                };

                var lastmessageJson = JsonConvert.SerializeObject(x, Formatting.Indented);
                File.WriteAllText(JsonLastMessageLocation, lastmessageJson);
            }
        }

        private static string Open()
        {
            Console.WriteLine("Open");
            byte[] file = File.ReadAllBytes(JsonMessageFileLocation);
            Console.WriteLine("file");
            using (var stream = new GZipStream(new MemoryStream(file), CompressionMode.Decompress))
            {
                Console.WriteLine("stream");
                const int size = 4096;
                byte[] buffer = new byte[size];

                using (MemoryStream memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    Console.WriteLine("return");
                    return Encoding.Default.GetString(memory.ToArray());
                }
            }
        }
    }

    struct JsonMessage
    {
        public string M { get; set; }
    }

    struct JsonLastMessage
    {
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
