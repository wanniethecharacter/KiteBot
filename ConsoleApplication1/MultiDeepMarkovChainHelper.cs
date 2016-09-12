using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Newtonsoft.Json;
using TextMarkovChains;

namespace KiteBot
{
    public class MultiTextMarkovChainHelper
    {
        public static int Depth;
        private static Timer _timer;
        private static IMarkovChain _markovChain;
        private static DiscordClient _client;
        private static bool _isInitialized;
        private static List<JsonMessage> _jsonList = new List<JsonMessage>();
        private static JsonLastMessage _lastMessage = new JsonLastMessage();

        public static string RootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        public static string JsonLastMessageLocation => RootDirectory + "/Content/LastMessage.json";
        public static string JsonMessageFileLocation => RootDirectory + "/Content/messages.zip";
        

        public MultiTextMarkovChainHelper(int depth) : this(Program.Client, depth)
        {
        }

        public MultiTextMarkovChainHelper(DiscordClient client,int depth)
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
            _timer.Elapsed += (s,e) => Save();
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
                            List<Message> list =
                                await DownloadMessagesAfterId(_lastMessage.MessageId,_client.GetChannel(_lastMessage.ChannelId));
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
                    List<Message> list = await GetMessagesFromChannel(_client.GetChannel(85842104034541568), 10000);
                    list.AddRange(await GetMessagesFromChannel(_client.GetChannel(96786127238725632), 5000));
                    list.AddRange(await GetMessagesFromChannel(_client.GetChannel(94122326802571264), 5000));
                    foreach (Message message in list)
                    {
                        if (message != null && !message.Text.Equals(""))
                        {
                            FeedMarkovChain(message);
                            var json = new JsonMessage{M = message.Text};
                            _jsonList.Add(json);
                        }
                    }
                }
                _timer.Enabled = true;
                return _isInitialized = true;
            }
            return _isInitialized;
        }

        private async Task<List<Message>> DownloadMessagesAfterId(ulong id, Channel channel)
        {
            Console.WriteLine("DownloadMessagesAfterId");
            List<Message> messages = new List<Message>();
            var latestMessages = await channel.DownloadMessages(100, id,Relative.After);
            messages.AddRange(latestMessages);

            if (latestMessages.Length == 0) return messages;

            ulong tmpMessageTracker = latestMessages.Last().Id;
            // ReSharper disable once TooWideLocalVariableScope
            ulong newMessageTracker;

            while (true)
            {
                messages.AddRange(await channel.DownloadMessages(100, tmpMessageTracker,Relative.After));

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

        public void Feed(Message message)
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

        private void FeedMarkovChain(Message message)
        {
            if (!message.User.IsBot)
            {
                if(!message.Text.Equals("") && !message.Text.Contains("http") && !message.Text.ToLower().Contains("testmarkov") && !message.Text.ToLower().Contains("getdunked") && !message.IsMentioningMe())
                {
                    if (message.Text.Contains("."))
                    {
                        if (GC.GetTotalMemory(false) < 512000000)
                            _markovChain.feed(message.Text);
                    }
                    _markovChain.feed(message.Text + ".");
                    _jsonList.Add(new JsonMessage { M = message.Text });
                }
            }
        }

        private async Task<List<Message>> GetMessagesFromChannel(Channel channel, int i)
        {
            Console.WriteLine("GetMessagesFromChannel");
            List<Message> messages = new List<Message>();
            var latestMessage = await channel.DownloadMessages(i);
            messages.AddRange(latestMessage);

            ulong tmpMessageTracker = latestMessage.Last().Id;

            while (messages.Count < i)
            {
                messages.AddRange(await channel.DownloadMessages(100, tmpMessageTracker));

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

        public void Save()
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
                Message message = _client.GetChannel(85842104034541568).DownloadMessages(1).Result[0];
                var x = new JsonLastMessage
                {
                    MessageId = message.Id,
                    ChannelId = message.Channel.Id
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
