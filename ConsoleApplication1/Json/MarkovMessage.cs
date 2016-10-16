using Newtonsoft.Json;

namespace KiteBot.Json
{

    internal class MarkovMessage
    {

        [JsonProperty("M")]
        public string M { get; set; }
    }

}
