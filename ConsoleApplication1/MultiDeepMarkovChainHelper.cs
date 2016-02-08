using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Discord;

namespace KiteBot
{
    public class MultiTextMarkovChainHelper
    {
        public static int Depth;
        private static MultiDeepMarkovChain multiDeep;
        private static DiscordClient _client;

        public static string RootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        public static string XmlFileLocation = RootDirectory + "\\Content\\MarkovChain.xml";
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
                if (File.Exists(path: XmlFileLocation))
                {
                    var XDoc = new XmlDocument();
                    XDoc.Load(XmlFileLocation);
                    multiDeep.feed(XDoc);
                    // ReSharper disable once RedundantAssignment
                    XDoc = null;
                }
                else
                {
                    List<Message> list = await GetMessagesFromChannel(_client, _client.GetChannel(85842104034541568), 50000);
                    list.AddRange(await GetMessagesFromChannel(_client, _client.GetChannel(96786127238725632), 5000));
                    list.AddRange(await GetMessagesFromChannel(_client, _client.GetChannel(94122326802571264), 5000));
                    foreach (Message message in list)
                    {
                        try
                        {
                            if (message != null && !message.User.Name.ToLower().Contains("kitebot"))
                            {
                                if (!message.Text.Equals("") &&
                                    !message.Text.Contains("http") &&
                                    !message.Text.ToLower().Contains("testmarkov") &&
                                    !message.IsMentioningMe)
                                {
                                    var lowerCaseMessage = message.Text.ToLower();
                                    if (!lowerCaseMessage.Contains("."))
                                    {
                                        lowerCaseMessage += ".";
                                    }
                                    multiDeep.feed(lowerCaseMessage);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine(message?.Text);
                        }
                    }
                }
                return _isInitialized = true;
            }
            return _isInitialized;
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
                if(!message.Text.Equals("") && !message.Text.Contains("http") && !message.Text.ToLower().Contains("testmarkov") && !message.IsMentioningMe)
                {
                    if (message.Text.Contains("."))
                    {
                        multiDeep.feed(message.Text);
                    }
                    multiDeep.feed(message.Text + ".");
                }
            }
        }

        private async Task<List<Message>> GetMessagesFromChannel(DiscordClient client, Channel channel, int i)
        {
            List<Message> messages = new List<Message>();
            var latestMessage = client.DownloadMessages(channel, i);
            messages.AddRange(client.DownloadMessages(channel, i).Result);

            long tmpMessageTracker = latestMessage.Id;

            while (messages.Count < i)
            {
                messages.AddRange(await Program.Client.DownloadMessages(channel, 100, tmpMessageTracker));

                long newMessageTracker = messages[messages.Count - 1].Id;
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

        public void save()
        {
            multiDeep.save(XmlFileLocation);
        }
    }
}
