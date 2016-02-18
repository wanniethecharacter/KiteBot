using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace KiteBot
{
    public class MultiTextMarkovChainHelper
    {
        public static int Depth;
        private static MultiDeepMarkovChain multiDeep;
        private static DiscordClient _client;
        private static List<JsonMessage> JsonList = new List<JsonMessage>();

        public static string RootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        public static string JsonFileLocation => RootDirectory + "\\Content\\messages" + Depth + ".json";
        private static bool _isInitialized;

        public MultiTextMarkovChainHelper(int depth) : this(Program.Client, depth)
        {
        }

        public MultiTextMarkovChainHelper(DiscordClient client,int depth)
        {
            _client = client;
            Depth = depth;
            multiDeep = new MultiDeepMarkovChain(Depth);
        }

        public async Task<bool> Initialize()
        {
            if (!_isInitialized)
            {
                if (File.Exists(path: JsonFileLocation))
                {
                    JsonList = JsonConvert.DeserializeObject<List<JsonMessage>>(File.ReadAllText(JsonFileLocation));
                    ulong id = JsonList.First(item => item.CI == 85842104034541568).MI;
                    foreach (JsonMessage message in JsonList)
                    {
                        multiDeep.feed(message.M);//Any messages here have already been thru all the if checks, and hence, we dont need to run thru all of those again.
                    }
                    _isInitialized = true;

                    List<Message> list = await DownloadMessagesAfterId(id, _client.GetChannel(85842104034541568));
                    foreach (Message message in list)
                    {
                        FeedMarkovChain(message);
                        var json = new JsonMessage
                        {
                            M = message.Text,
                            MI = message.Id,
                            CI = message.Channel.Id,
                        };
                        JsonList.Add(json);
                    }
                }
                else
                {
                    List<Message> list = await GetMessagesFromChannel(_client.GetChannel(85842104034541568), 200000);
                    list.AddRange(await GetMessagesFromChannel(_client.GetChannel(96786127238725632), 10000));
                    list.AddRange(await GetMessagesFromChannel(_client.GetChannel(94122326802571264), 10000));
                    foreach (Message message in list)
                    {
                        if (message != null && !message.Text.Equals(""))
                        {
                            FeedMarkovChain(message);
                            var json = new JsonMessage
                            {
                                M = message.Text,
                                MI = message.Id,
                                CI = message.Channel.Id,
                            };
                            JsonList.Add(json);
                        }
                    }
                }
                return _isInitialized = true;
            }
            return _isInitialized;
        }

        private async Task<List<Message>> DownloadMessagesAfterId(ulong id, Channel channel)
        {
            List<Message> messages = new List<Message>();
            var latestMessage = await channel.DownloadMessages(100, id,Relative.After);
            messages.AddRange(latestMessage);

            ulong tmpMessageTracker = latestMessage.Last().Id;
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
                    messages.RemoveAt(messages.Count - 1);//removes the excessive object
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
                return multiDeep.generateSentence();
            }
            return "I'm not ready yet Senpai!";
        }

        private void FeedMarkovChain(Message message)
        {
            if (!message.User.Name.ToLower().Contains("kitebot"))
            {
                if(!message.Text.Equals("") && !message.Text.Contains("http") && !message.Text.ToLower().Contains("testmarkov") && !message.Text.ToLower().Contains("getdunked") && !message.IsMentioningMe())
                {
                    if (message.Text.Contains("."))
                    {
                        multiDeep.feed(message.Text);
                    }
                    multiDeep.feed(message.Text + ".");
                }
            }
        }

        private async Task<List<Message>> GetMessagesFromChannel(Channel channel, int i)
        {
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
                    messages.RemoveAt(messages.Count - 1);//removes the excessive object
                    return messages;
                }
            }
            return messages;
        }

        public void Save()
        {
            File.WriteAllText(JsonFileLocation, JsonConvert.SerializeObject(JsonList, Newtonsoft.Json.Formatting.None));
        }
    }

    class JsonMessage
    {
        public string M { get; set; }
        public ulong MI { get; set; }
        public ulong CI { get; set; }
    }
}
