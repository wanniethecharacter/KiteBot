using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace KiteBot
{
    class TextMarkovChainHelper
    {
        //ARRAY
        private static Dictionary<long,List<Message>> channelMessages = new Dictionary<long,List<Message>>();

        public TextMarkovChainHelper() : this(Program.Client, 100)
        {
        }

        public TextMarkovChainHelper(DiscordClient client,int messageCount)
        {
            var channels = client.AllServers.GetEnumerator().Current.Channels;
            foreach (var channel in channels)
            {
                channelMessages.Add(channel.Id,GetMessageLog(client, channel, messageCount).Result);
            }
        }

        private async Task<List<Message>> GetMessageLog(DiscordClient client, Channel channel, int i)
        {
            List<Message> messages = new List<Message>();
            messages.AddRange(client.DownloadMessages(channel, i).Result);

            while (messages.Count <= i)
            {
                await client.DownloadMessages(channel, (i-messages.Count),messages[messages.Count-1].Id);//the number doesnt matter as long as its over 100 cause the function limits it to 100 regardless
            }
            return messages;
        }
    }
}
