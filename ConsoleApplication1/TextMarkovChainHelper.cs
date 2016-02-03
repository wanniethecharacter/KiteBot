using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace KiteBot
{
    public class TextMarkovChainHelper
    {
        private const int MaxMessages = 1500;
        private static readonly Dictionary<long,List<Message>> ChannelMessages = 
            new Dictionary<long,List<Message>>();
        private static readonly Dictionary<long, TextMarkovChain> ChannelMarkovChains =
            new Dictionary<long, TextMarkovChain>();   
        private static DiscordClient _client;
        private static bool _isInitialized;

        public TextMarkovChainHelper() : this(Program.Client)
        {
        }

        public TextMarkovChainHelper(DiscordClient client)
        {
            _client = client;
        }

        public async Task<bool> Initialize()
        {
            if (!_isInitialized)
            {
                List<long> channelIds = new List<long>();
                foreach (Channel channel in _client.AllServers.SelectMany(server => server.Channels).Where(channel => ChannelType.Text == channel.Type))
                {
                    if (channel.Members.Contains(_client.GetUser(channel.Server,_client.CurrentUser.Id)))
                    {
                        channelIds.Add(channel.Id);
                        ChannelMarkovChains.Add(channel.Id, new TextMarkovChain());
                        ChannelMessages.Add(channel.Id, new List<Message>());
                    }
                }
                _isInitialized = true;

                foreach (long id in channelIds)
                {
                    ChannelMessages[id].AddRange(await GetMessagesFromChannel(Program.Client, Program.Client.GetChannel(id), MaxMessages));
                }
            }
            return _isInitialized;
        }


        public void Feed(Message message)
        {
            if (ChannelMessages.ContainsKey(message.Channel.Id))
            {
                ChannelMessages[message.Channel.Id].Add(message);

                if (ChannelMarkovChains.ContainsKey(message.Channel.Id) && ChannelMarkovChains[message.Channel.Id].readyToGenerate())
                {
                    FeedMarkovChain(ChannelMarkovChains[message.Channel.Id], message);
                }
            }
            else
            {
                ChannelMessages.Add(message.Channel.Id, GetMessagesFromChannel(_client, message.Channel, MaxMessages).Result);
            }
        }

        public async Task<string> GetSequenceForChannel(Channel channel)
        {
            if (_isInitialized)
            {
                TextMarkovChain textMarkovChain;
                if (ChannelMarkovChains.TryGetValue(channel.Id, out textMarkovChain))
                {
                    foreach (Message message in ChannelMessages[channel.Id])
                    {
                        FeedMarkovChain(textMarkovChain,message);
                    }
                    ChannelMessages[channel.Id].Clear();
                    if (textMarkovChain.readyToGenerate())
                    {
                        return textMarkovChain.generateSentence().Result;
                    }
                }
                else
                {
                    textMarkovChain = new TextMarkovChain();
                    List<Message> newList = await GetMessagesFromChannel(Program.Client, channel, MaxMessages);
                    foreach (Message message in newList)
                    {
                        FeedMarkovChain(textMarkovChain, message);
                    }
                    ChannelMarkovChains.Add(channel.Id, textMarkovChain);
                    if (textMarkovChain.readyToGenerate())
                    {
                        return textMarkovChain.generateSentence().Result;
                    }
                }
            }
            return "I'm not ready yet Senpai!";
        }

        public async Task<string> GetSequenceForChannel(Channel channel,string input)
        {
            if (_isInitialized)
            {
                TextMarkovChain textMarkovChain;
                if (ChannelMarkovChains.TryGetValue(channel.Id, out textMarkovChain))
                {
                    foreach (Message message in ChannelMessages[channel.Id])
                    {
                        FeedMarkovChain(textMarkovChain, message);
                    }
                    ChannelMessages[channel.Id].Clear();
                    if (textMarkovChain.readyToGenerate())
                    {
                        return textMarkovChain.generateSentence(input).Result;
                    }
                }
                else
                {
                    textMarkovChain = new TextMarkovChain();
                    List<Message> newList = await GetMessagesFromChannel(Program.Client, channel, MaxMessages);
                    ChannelMessages.Add(channel.Id,newList);
                    foreach (Message message in newList)
                    {
                        FeedMarkovChain(textMarkovChain, message);
                    }
                    ChannelMarkovChains.Add(channel.Id, textMarkovChain);
                    if (textMarkovChain.readyToGenerate())
                    {
                        return textMarkovChain.generateSentence(input).Result;
                    }
                }
            }
            return "I'm not ready yet Senpai!";
        }

        private void FeedMarkovChain(TextMarkovChain textMarkovChain, Message message)
        {
            if (!message.User.Name.ToLower().Contains("kitebot"))
            {
                if(!message.Text.Equals("") && !message.Text.Contains("http") && !message.Text.ToLower().Contains("testmarkov"))
                {
                    textMarkovChain.feed(message.Text);
                }
            }
        }

        private async Task<List<Message>> GetMessagesFromChannel(DiscordClient client, Channel channel, int i)
        {
            List<Message> messages = new List<Message>();
            var latestMessage = client.DownloadMessages(channel, i);
            messages.AddRange(client.DownloadMessages(channel, i).Result);

            long tmpMessageTracker = latestMessage.Id;

            while (messages.Count < MaxMessages)
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
    }
}
