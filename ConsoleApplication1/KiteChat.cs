using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

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
		//public static KitCoGame KiteGame = new KitCoGame();
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

        public async Task AsyncParseChat(object s, MessageEventArgs e, DiscordClient client)
        {
            Console.WriteLine("(" + e.User.Name + "/" + e.User.Id + ") - " + e.Message.Text);
            IsRaeTyping(e);

            //add all messages to the Markov Chain list

            if (!e.Message.IsAuthor)
            {
                MultiDeepMarkovChains.Feed(e.Message);

                if (e.Message.Text.StartsWith("/anime"))
                {
                    try
                    {
                        var result = await SearchHelper.GetAnimeData(e.Message.Text.Remove(0, 6));
                        await e.Channel.SendMessage(result.ToString());
                    }
                    catch (Exception)
                    {
                        await e.Channel.SendMessage("Can't find any good anime named anything like that.");
                    }
                }
                else if (e.Message.Text.StartsWith("/manga"))
                {
                    try
                    {
                        var result = await SearchHelper.GetMangaData(e.Message.Text.Remove(0, 6));
                        await e.Channel.SendMessage(result.ToString());
                    }
                    catch (Exception)
                    {
                        await e.Channel.SendMessage("Why are you even looking for manga when there is anime.");
                    }
                }
                else if (e.Message.Text.Contains("Mistake") && e.Channel.Id == 96786127238725632)
                {
                    await e.Channel.SendMessage("Anime is a mistake " + e.User.Mention +".");
                }
                else if (e.Message.Text.StartsWith("/roll"))
                {
                    await e.Channel.SendMessage(DiceRoller.ParseRoll(e.Message.Text));
                }

                else if (e.Message.Text.ToLower().StartsWith("!reminder"))
                {
                    await e.Channel.SendMessage(Reminder.AddNewEvent(e.Message));
                }

                else if (e.Message.Text.ToLower().StartsWith("!whois"))
                {
                    var userMentioned =
                        e.Message.MentionedUsers.FirstOrDefault(x => x.Id != Program.Client.CurrentUser.Id);
                    if (userMentioned != null)
                    {
                        await
                            e.Channel.SendMessage(
                                $"Former names for {userMentioned.Name} are: {EnumWhoIs(userMentioned.Id)}.".Replace(
                                    ",.", "."));
                    }
                }

                else if (e.Message.Text.Contains("GetDunked"))
                {
                    await e.Channel.SendMessage("http://i.imgur.com/QhcNUWo.gif");
                }

                else if (e.Message.Text.Contains(@"/saveJSON") && e.User.Id == 85817630560108544 && StartMarkovChain)
                {
                    MultiDeepMarkovChains.Save();
                    await e.Channel.SendMessage("Done.");
                }
                else if (e.Message.Text.Contains(@"/saveExit") && e.User.Id == 85817630560108544 && StartMarkovChain)
                {
                    MultiDeepMarkovChains.Save();
                    await e.Channel.SendMessage("Done.");
                    Environment.Exit(1);
                }
                else if (e.Message.Text.Contains(@"/restart") && e.User.Id == 85817630560108544)
                {
                    StreamChecker.Restart();
                    GbVideoChecker.Restart();
                }
                else if ((e.Message.Text.Contains(@"/testMarkov") || e.Message.Text.StartsWith(@"@KiteBot /tm")) &&
                         StartMarkovChain)
                {
                    try
                    {
                        await e.Channel.SendMessage(MultiDeepMarkovChains.GetSequence());
                    }
                    catch (Exception)
                    {
                        MultiDeepMarkovChains.Save();
                        throw;
                    }
                    finally
                    {
                        if (e.User.Id == 85817630560108544)
                        {
                            await e.Message.Delete();
                        }
                    }
                }

                else if (e.Message.IsMentioningMe())
                {
                    if (e.Message.Text.StartsWith("@KiteBot #420") ||
                        e.Message.Text.ToLower().StartsWith("@KiteBot #blaze") ||
                        e.Message.Text.ToLower().Contains("waifu"))
                    {
                        await e.Channel.SendMessage("http://420.moe/");
                    }
                    else if (e.Message.Text.ToLower().Contains("help"))
                    {
                        var nl = Environment.NewLine;
                        await e.Channel.SendMessage("Current Commands are:" + nl + 
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
                    else if (e.Message.Text.ToLower().Contains("randomql"))
                    {
                        await
                            e.Channel.SendMessage(GetResponseUriFromRandomQlCrew());
                    }
                    else if (e.Message.Text.ToLower().Contains("raecounter"))
                    {
                        await e.Channel.SendMessage(@"Rae has ghost-typed " + RaeCounter);
                    }
                    else if (e.Message.Text.ToLower().Contains("google"))
                    {
                        await
                            e.Channel.SendMessage("http://lmgtfy.com/?q=" +
                                                  e.Message.Text.ToLower().Substring(16).Replace(' ', '+'));
                    }
                    else if (e.Message.Text.ToLower().Contains("youtube"))
                    {
                        if (e.Message.Text.Length > 16)
                        {
                            await e.Channel.SendMessage("https://www.youtube.com/results?search_query=" +
                                                        e.Message.Text.ToLower().Substring(17).Replace(' ', '+'));
                        }
                        else
                        {
                            await e.Channel.SendMessage("Please add a query after youtube, starting with a space.");
                        }
                    }                    
                    else if (e.Message.Text.ToLower().Contains("fuck you") ||
                             e.Message.Text.ToLower().Contains("fuckyou"))
                    {
                        List<string> possibleResponses = new List<string>
                        {
                            "Hey fuck you too USER!",
                            "I bet you'd like that wouldn't you USER?",
                            "No, fuck you USER!",
                            "Fuck you too USER!"
                        };

                        await
                            e.Channel.SendMessage(
                                possibleResponses[RandomSeed.Next(0, possibleResponses.Count)].Replace("USER",
                                    e.User.Name));
                    }
                    else if (e.Message.Text.ToLower().Contains("/pizza"))
                    {
                        await e.Channel.SendMessage(KitePizza.ParsePizza(e.User.Name, e.Message.Text));
                    }
                    else if (e.Message.Text.ToLower().Contains("sandwich"))
                    {
                        await e.Channel.SendMessage(KiteSandwich.ParseSandwich(e.User.Name));
                    }
                    else if (e.Message.Text.ToLower().Contains("hi") ||
                             e.Message.Text.ToLower().Contains("hey") ||
                             e.Message.Text.ToLower().Contains("hello"))
                    {
                        await e.Channel.SendMessage(ParseGreeting(e.User.Name));
                    }
                    else if (0 <= e.Message.Text.ToLower().IndexOf("/meal", 0, StringComparison.Ordinal) ||
                             e.Message.Text.ToLower().Contains("dinner")
                             || e.Message.Text.ToLower().Contains("lunch"))
                    {
                        await
                            e.Channel.SendMessage(
                                _mealResponses[RandomSeed.Next(0, _mealResponses.Length)].Replace("USER",
                                    e.User.Name));
                    }
                    else
                    {
                        await
                            e.Channel.SendMessage("KiteBot ver. 1.1.3 \"Now with real dairy.\"");
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

        public void IsRaeTyping(MessageEventArgs e)
        {
            if (e.User.Id == 85876755797139456)
            {
                RaeCounter += -1;
            }
        }

        public void IsRaeTyping(ChannelUserEventArgs channelUserEventArgs)
        {
            if (channelUserEventArgs.User.Id == 85876755797139456)
            {
                RaeCounter += 1;
            }
        }

        public void AddWhoIs(UserUpdatedEventArgs e)
        {
            if (WhoIsDictionary.ContainsKey(e.After.Id))
            {
                WhoIsDictionary[e.Before.Id].OldNames.Add(e.After.Name);
            }
            else
            {
                string[] names = {e.Before.Name,e.After.Name};
                WhoIsDictionary.Add(e.Before.Id, new WhoIsPerson
                {
                    UserId = e.Before.Id,
                    OldNames = new List<string>(names)
                });
            }
            File.WriteAllText(WhoIsLocation,JsonConvert.SerializeObject(WhoIsDictionary));
        }

        public void AddWhoIs(UserUpdatedEventArgs e, string nicknameAfter)
        {
            if (WhoIsDictionary.ContainsKey(e.After.Id))
            {
                WhoIsDictionary[e.Before.Id].OldNames.Add(nicknameAfter);
            }
            else
            {
                string[] names = { e.Before.Name, nicknameAfter };
                WhoIsDictionary.Add(e.Before.Id, new WhoIsPerson
                {
                    UserId = e.Before.Id,
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
