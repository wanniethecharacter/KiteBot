using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.WebSocket;

namespace KiteBot
{
    public class KiteChat
    {
        //private static Timer _chatTimer;
        public static Random RandomSeed;

		public static int RaeCounter;
        public static bool StartMarkovChain;

        private static string[] _greetings;
        private static string[] _mealResponses;
        private static string[] _bekGreetings;

        public static KitePizza KitePizza = new KitePizza();
        public static KiteSandwich KiteSandwich = new KiteSandwich();
		public static KiteDunk KiteDunk = new KiteDunk();
		public static DiceRoller DiceRoller = new DiceRoller();
        public static LivestreamChecker StreamChecker;
        public static GiantBombVideoChecker GbVideoChecker;
        public static MultiTextMarkovChainHelper MultiDeepMarkovChains;
        public static Dictionary<ulong,WhoIsPerson> WhoIsDictionary; 

        public static string ChatDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        public static string GreetingFileLocation = ChatDirectory + "/Content/Greetings.txt";
        public static string MealFileLocation = ChatDirectory + "/Content/Meals.txt";
        public static string WhoIsLocation = ChatDirectory + "/Content/Whois.json";


        public KiteChat(bool markovbool, string gBapi, int streamRefresh, int videoRefresh, int depth) : this(markovbool, depth,gBapi, streamRefresh, videoRefresh, File.ReadAllLines(GreetingFileLocation), File.ReadAllLines(MealFileLocation), new Random())
        {
        }

        public KiteChat(bool markovbool, int depth, string gBapi,int streamRefresh, int videoRefresh, string[] arrayOfGreetings, string[] arrayOfMeals, Random randomSeed)
        {
            StartMarkovChain = markovbool;
            _greetings = arrayOfGreetings;
            _mealResponses = arrayOfMeals;
            RandomSeed = randomSeed;
            RaeCounter = 0;

            StreamChecker = new LivestreamChecker(gBapi, streamRefresh);
            GbVideoChecker = new GiantBombVideoChecker(gBapi, videoRefresh);
            MultiDeepMarkovChains = new MultiTextMarkovChainHelper(depth);
            WhoIsDictionary = File.Exists(WhoIsLocation)
                ? JsonConvert.DeserializeObject<Dictionary<ulong, WhoIsPerson>>(File.ReadAllText(WhoIsLocation))
                : new Dictionary<ulong, WhoIsPerson>();
        }

        public async Task<bool> InitializeMarkovChain()
        {
            var bek = LoadBekGreetings();
            if (StartMarkovChain) {await MultiDeepMarkovChains.Initialize();}
            return await bek;
        }

        public async Task AsyncParseChat(IMessage msg, IDiscordClient client)
        {
            Console.WriteLine("(" + msg.Author.Username + "/" + msg.Author.Id + ") - " + msg.Content);
            IsRaeTyping(msg);

            //add all messages to the Markov Chain list

            if (msg.Author.Id != client.CurrentUser.Id)
            {
                MultiDeepMarkovChains.Feed(msg);

                if (msg.Content.StartsWith("/anime"))
                {
                    try
                    {
                        var result = await SearchHelper.GetAnimeData(msg.Content.Remove(0, 6));
                        await msg.Channel.SendMessageAsync(result.ToString());
                    }
                    catch (Exception)
                    {
                        await msg.Channel.SendMessageAsync("Can't find any good anime named anything like that.");
                    }
                }
                else if (msg.Content.StartsWith("/manga"))
                {
                    try
                    {
                        var result = await SearchHelper.GetMangaData(msg.Content.Remove(0, 6));
                        await msg.Channel.SendMessageAsync(result.ToString());
                    }
                    catch (Exception)
                    {
                        await msg.Channel.SendMessageAsync("Why are you even looking for manga when there is anime.");
                    }
                }
                else if (msg.Content.Contains("Mistake") && msg.Channel.Id == 96786127238725632)
                {
                    await msg.Channel.SendMessageAsync("Anime is a mistake " + msg.Author.Mention +".");
                }
                else if (msg.Content.StartsWith("/roll"))
                {
                    await msg.Channel.SendMessageAsync(DiceRoller.ParseRoll(msg.Content));
                }

                else if (msg.Content.ToLower().StartsWith("!reminder"))
                {
                    await msg.Channel.SendMessageAsync(Reminder.AddNewEvent(msg));
                }

                else if (msg.Content.ToLower().StartsWith("!whois"))
                {
                    var userMentioned = msg.MentionedUserIds.FirstOrDefault();
                    if (userMentioned != 0)
                    {
                        await
                            msg.Channel.SendMessageAsync(
                                $"Former names for {await client.GetUserAsync(userMentioned)} are: {EnumWhoIs(userMentioned)}.".Replace(
                                    ",.", "."));
                    }
                }

                else if (msg.Content.Contains("GetDunked"))
                {
                    await msg.Channel.SendMessageAsync("http://i.imgur.com/QhcNUWo.gif");
                }

                else if (msg.Content.Contains(@"/saveJSON") && msg.Author.Id == 85817630560108544 && StartMarkovChain)
                {
                    await MultiDeepMarkovChains.Save();
                    await msg.Channel.SendMessageAsync("Done.");
                }
                else if (msg.Content.Contains(@"/saveExit") && msg.Author.Id == 85817630560108544 && StartMarkovChain)
                {
                    await MultiDeepMarkovChains.Save();
                    await msg.Channel.SendMessageAsync("Done.");
                    Environment.Exit(1);
                }
                else if (msg.Content.Contains(@"/restart") && msg.Author.Id == 85817630560108544)
                {
                    StreamChecker.Restart();
                    GbVideoChecker.Restart();
                }
                else if ((msg.Content.Contains(@"/testMarkov") || msg.Content.StartsWith(@"@KiteBot /tm")) &&
                         StartMarkovChain)
                {
                    try
                    {
                        await msg.Channel.SendMessageAsync(MultiDeepMarkovChains.GetSequence());
                    }
                    catch (Exception)
                    {
                        await MultiDeepMarkovChains.Save();
                        throw;
                    }
                    finally
                    {
                        if (msg.Author.Id == 85817630560108544)
                        {
                            var userMessage = (IUserMessage) msg;
                            await userMessage.DeleteAsync();
                        }
                    }
                }

                else if (msg.Content.Contains(@"@KiteBot"))
                {
                    if (msg.Content.StartsWith("@KiteBot #420") ||
                        msg.Content.ToLower().StartsWith("@KiteBot #blaze") ||
                        msg.Content.ToLower().Contains("waifu"))
                    {
                        await msg.Channel.SendMessageAsync("http://420.moe/");
                    }
                    else if (msg.Content.ToLower().Contains("help"))
                    {
                        var nl = Environment.NewLine;
                        await msg.Channel.SendMessageAsync("Current Commands are:" + nl + 
                                                    "#420"+ nl + 
                                                    "randomql" + nl + 
                                                    "google" + nl + 
                                                    "youtube" + nl +                                                    
                                                    "/anime" + nl +
                                                    "/manga" + nl +
                                                    "!reminder" + nl +
                                                    "/pizza" + nl + 
                                                    "Whats for dinner" + nl + 
                                                    "sandwich" + nl +
                                                    "RaeCounter"
                                                    + nl + "help");
                    }
                    else if (msg.Content.ToLower().Contains("randomql"))
                    {
                        await
                            msg.Channel.SendMessageAsync(GetResponseUriFromRandomQlCrew());
                    }
                    else if (msg.Content.ToLower().Contains("raecounter"))
                    {
                        await msg.Channel.SendMessageAsync(@"Rae has ghost-typed " + RaeCounter);
                    }
                    else if (msg.Content.ToLower().Contains("google"))
                    {
                        await
                            msg.Channel.SendMessageAsync("http://lmgtfy.com/?q=" +
                                                  msg.Content.ToLower().Substring(16).Replace(' ', '+'));
                    }
                    else if (msg.Content.ToLower().Contains("youtube"))
                    {
                        if (msg.Content.Length > 16)
                        {
                            await msg.Channel.SendMessageAsync("https://www.youtube.com/results?search_query=" +
                                                        msg.Content.ToLower().Substring(17).Replace(' ', '+'));
                        }
                        else
                        {
                            await msg.Channel.SendMessageAsync("Please add a query after youtube, starting with a space.");
                        }
                    }                    
                    else if (msg.Content.ToLower().Contains("fuck you") ||
                             msg.Content.ToLower().Contains("fuckyou"))
                    {
                        List<string> possibleResponses = new List<string>
                        {
                            "Hey fuck you too USER!",
                            "I bet you'd like that wouldn't you USER?",
                            "No, fuck you USER!",
                            "Fuck you too USER!"
                        };

                        await
                            msg.Channel.SendMessageAsync(
                                possibleResponses[RandomSeed.Next(0, possibleResponses.Count)].Replace("USER",
                                    msg.Author.Username));
                    }
                    else if (msg.Content.ToLower().Contains("/pizza"))
                    {
                        await msg.Channel.SendMessageAsync(KitePizza.ParsePizza(msg.Author.Username, msg.Content));
                    }
                    else if (msg.Content.ToLower().Contains("sandwich"))
                    {
                        await msg.Channel.SendMessageAsync(KiteSandwich.ParseSandwich(msg.Author.Username));
                    }
                    else if (msg.Content.ToLower().Contains("hi") ||
                             msg.Content.ToLower().Contains("hey") ||
                             msg.Content.ToLower().Contains("hello"))
                    {
                        await msg.Channel.SendMessageAsync(ParseGreeting(msg.Author.Username));
                    }
                    else if (0 <= msg.Content.ToLower().IndexOf("/meal", 0, StringComparison.Ordinal) ||
                             msg.Content.ToLower().Contains("dinner")
                             || msg.Content.ToLower().Contains("lunch"))
                    {
                        await
                            msg.Channel.SendMessageAsync(
                                _mealResponses[RandomSeed.Next(0, _mealResponses.Length)].Replace("USER",
                                    msg.Author.Username));
                    }
                    else
                    {
                        await
                            msg.Channel.SendMessageAsync("KiteBot ver. 1.1.3 \"Now with real dairy.\"");
                    }
                }
            }
        }

        public static string GetResponseUriFromRandomQlCrew()
		{
            string url = "http://qlcrew.com/main.php?anyone=anyone&inc%5B0%5D=&p=999&exc%5B0%5D=&per_page=15&random";

            /*WebClient client = new WebClient();
            client.Headers.Add("user-agent", "LassieMEKiteBot/0.9 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
		    client..OpenRead(url);*/

		    HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                request.UserAgent = "LassieMEKiteBot/0.11 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response.ResponseUri.AbsoluteUri;
            }
            return "Couldn't load qlcrew's Random Link.";
		}
        
        //returns a greeting from the greetings.txt list on a per user or generic basis
	    private string ParseGreeting(string userName)
        {
		    if (userName.Equals("Bekenel") || userName.Equals("Pete"))
		    {
			    return (_bekGreetings[RandomSeed.Next(0, _bekGreetings.Length)]);
		    }
	        List<string> possibleResponses = new List<string>();

	        for (int i = 0; i < _greetings.Length - 2; i += 2)
	        {
	            if (userName.ToLower().Contains(_greetings[i]))
	            {
	                possibleResponses.Add(_greetings[i + 1]);
	            }
	        }

	        if (possibleResponses.Count == 0)
	        {
	            for (int i = 0; i < _greetings.Length - 2; i += 2)
	            {
	                if (_greetings[i] == "generic")
	                {
	                    possibleResponses.Add(_greetings[i + 1]);
	                }
	            }
	        }

	        //return a random response from the context provided, replacing the string "USER" with the appropriate username
	        return (possibleResponses[RandomSeed.Next(0, possibleResponses.Count)].Replace("USER", userName));
        }

        //grabs random greetings for user bekenel from a reddit profile
		private async Task<bool> LoadBekGreetings()
		{
			const string url = "https://www.reddit.com/user/UWotM8_SS";
			string htmlCode = null;
		    try
		    {
		        using (WebClient client = new WebClient())
		        {
		            htmlCode = await client.DownloadStringTaskAsync(url);
		        }
		    }
		    catch (Exception e)
		    {
		        Console.WriteLine("Could not load Bek greetings, server not found: " + e.Message);
		    }
		    finally
		    {
		        var regex1 = new Regex(@"<div class=""md""><p>(?<quote>.+)</p>");
		        if (htmlCode != null)
		        {
		            var matches = regex1.Matches(htmlCode);
		            var stringArray = new string[matches.Count];
		            var i = 0;
		            foreach (Match match in matches)
		            {
		                var s = match.Groups["quote"].Value.Replace("&#39;", "'").Replace("&quot;", "\"");
		                stringArray[i] = s;
		                i++;
		            }
		            _bekGreetings = stringArray;
                }
		    }
            return true;
        }

        public void IsRaeTyping(IMessage msg)
        {
            if (msg.Author.Id == 85876755797139456)
            {
                RaeCounter += -1;
            }
        }

        public void IsRaeTyping(IUser user)
        {
            if (user.Id == 85876755797139456)
            {
                RaeCounter += 1;
            }
        }

        public void AddWhoIs(IUser before, IUser after)
        {
            if (WhoIsDictionary.ContainsKey(after.Id))
            {
                WhoIsDictionary[after.Id].OldNames.Add(after.Username);
            }
            else
            {
                string[] names = {before.Username, after.Username};
                WhoIsDictionary.Add(after.Id, new WhoIsPerson
                {
                    UserId = after.Id,
                    OldNames = new List<string>(names)
                });
            }
            File.WriteAllText(WhoIsLocation,JsonConvert.SerializeObject(WhoIsDictionary));
        }

        public void AddWhoIs(IUser user, string nicknameAfter)
        {
            if (WhoIsDictionary.ContainsKey(user.Id))
            {
                WhoIsDictionary[user.Id].OldNames.Add(nicknameAfter);
            }
            else
            {
                string[] names = { user.Username, nicknameAfter };
                WhoIsDictionary.Add(user.Id, new WhoIsPerson
                {
                    UserId = user.Id,
                    OldNames = new List<string>(names)
                });
            }
            File.WriteAllText(WhoIsLocation, JsonConvert.SerializeObject(WhoIsDictionary));
        }

        public string EnumWhoIs(ulong id)
        {
            WhoIsPerson person;
            if (WhoIsDictionary.TryGetValue(id, out person))
            {
                var output = "";
                var list = person.OldNames;
                foreach (var name in list)
                {
                    output += $"{name},";
                }
                return output;
            }
            return "No former names found,";
        }

        public class WhoIsPerson
        {
            public ulong UserId { get; set; }
            public List<string> OldNames { get; set; }
        }
    }
}
