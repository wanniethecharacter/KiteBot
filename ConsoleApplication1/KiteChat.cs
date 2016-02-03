using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;

namespace KiteBot
{
    public class KiteChat
    {
        //private static Timer _chatTimer;
        public static Random RandomSeed;

		public static int RaeCounter;

        private static string[] _greetings;
        private static string[] _responses;
        private static string[] _mealResponses;
        private static string[] _bekGreetings;

        public static KitePizza KitePizza = new KitePizza();
        public static KiteSandwich KiteSandwich = new KiteSandwich();
		public static KiteDunk KiteDunk = new KiteDunk();
		public static GiantBombRss GiantBombRss = new GiantBombRss();
		public static DiceRoller DiceRoller = new DiceRoller();
		public static KitCoGame KiteGame = new KitCoGame();
		public static LivestreamChecker StreamChecker = new LivestreamChecker();
        public static TextMarkovChainHelper TextMarkovChainHelper = new TextMarkovChainHelper();

        public static string ChatDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.FullName;
        public static string GreetingFileLocation = ChatDirectory + "\\Content\\Greetings.txt";
        public static string ResponseFileLocation = ChatDirectory + "\\Content\\Responses.txt";
        public static string MealFileLocation = ChatDirectory + "\\Content\\Meals.txt";


        public KiteChat() : this(File.ReadAllLines(GreetingFileLocation), File.ReadAllLines(ResponseFileLocation),
                                File.ReadAllLines(MealFileLocation), new Random(DateTime.Now.Millisecond))
        {
        }

        public KiteChat(string[] arrayOfGreetings, string[] arrayOfResponses, string[] arrayOfMeals, Random randomSeed)
        {
			LoadBekGreetings();
            _greetings = arrayOfGreetings;
            _responses = arrayOfResponses;
            _mealResponses = arrayOfMeals;
            RandomSeed = randomSeed;
	        RaeCounter = 0;
        }

        public async Task AsyncParseChat(object s, MessageEventArgs e, DiscordClient client)
	    {
			Console.WriteLine("(" + e.User.Name + "/" + e.User.Id + ") - " + e.Message.Text);
		    IsRaeTyping(e);

            //add all messages to the Markov Chain list
            TextMarkovChainHelper.Feed(e.Message);

            if (e.Channel.Name.ToLower().Contains("vinncorobocorps"))
			{
				string response = KiteGame.GetGameResponse(e.Message);
				if (response != null)
					await client.SendMessage(e.Channel, response);
			}

			else if (!e.Message.IsAuthor && e.Message.Text.StartsWith("/roll"))
			{
				await client.SendMessage(e.Channel, DiceRoller.ParseRoll(e.Message.Text));
			}

			else if (!e.Message.IsAuthor && 0 <= e.Message.Text.IndexOf("GetDunked"))
			{
				await client.SendMessage(e.Channel, "http://i.imgur.com/QhcNUWo.gifv");
			}

			else if (!e.Message.IsAuthor && e.Message.Text.StartsWith(@"@KiteBot /forceUpdate"))
			{
				GiantBombRss.UpdateFeeds();
			}
            else if (!e.Message.IsAuthor && e.Message.Text.StartsWith(@"@KiteBot /testMarkov "))
            {
                await client.SendMessage(e.Channel, await TextMarkovChainHelper.GetSequenceForChannel(e.Channel,e.Message.Text.Substring(21).ToLower()));//this is bad
            }
            else if (!e.Message.IsAuthor && e.Message.Text.StartsWith(@"@KiteBot /testMarkov"))
            {
                await client.SendMessage(e.Channel, await TextMarkovChainHelper.GetSequenceForChannel(e.Channel));
            }

            else if (!e.Message.IsAuthor && e.Message.Text.StartsWith("@KiteBot"))
            {
                if (e.Message.Text.StartsWith("@KiteBot #420") || e.Message.Text.ToLower().StartsWith("@KiteBot #blaze") ||
                    0 <= e.Message.Text.ToLower().IndexOf("waifu", 0))
                {
                    await client.SendMessage(e.Channel, "http://420.moe/");
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("help", 5))
                {
                    var nl = Environment.NewLine;
                    await client.SendMessage(e.Channel, "Current Commands are:" + nl + "#420"
                                                        + nl + "randomql" + nl + "google" + nl + "youtube" + nl +
                                                        "kitedunk"
                                                        + nl + "/pizza" + nl + "Whats for dinner" + nl + "sandwich" + nl +
                                                        "RaeCounter"
                                                        + nl + "help");
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("randomql", 5))
                {
                    await
                        client.SendMessage(e.Channel,
                            GetResponseUriFromRandomQlCrew());
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("raecounter", 0))
                {
                    await client.SendMessage(e.Channel, @"Rae has ghost-typed " + RaeCounter);
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("google", 0))
                {
                    await
                        client.SendMessage(e.Channel,
                            "http://lmgtfy.com/?q=" + e.Message.Text.ToLower().Substring(16).Replace(' ', '+'));
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("youtube", 0))
                {
                    if (e.Message.Text.Length > 16)
                    {
                        await client.SendMessage(e.Channel,
                            "https://www.youtube.com/results?search_query=" +
                            e.Message.Text.ToLower().Substring(17).Replace(' ', '+'));
                    }
                    else
                    {
                        await client.SendMessage(e.Channel, "Please add a query after youtube, starting with a space.");
                    }
                }

                else if (0 <= e.Message.Text.ToLower().IndexOf("dunk", 0))
                {
                    await client.SendMessage(e.Channel, KiteDunk.GetUpdatedKiteDunk());
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("fuck you", 0) ||
                         0 <= e.Message.Text.ToLower().IndexOf("fuckyou", 0))
                {
                    List<string> _possibleResponses = new List<string>();
                    _possibleResponses.Add("Hey fuck you too USER!");
                    _possibleResponses.Add("I bet you'd like that wouldn't you USER?");
                    _possibleResponses.Add("No, fuck you USER!");
                    _possibleResponses.Add("Fuck you too USER!");

                    await
                        client.SendMessage(e.Channel,
                            _possibleResponses[RandomSeed.Next(0, _possibleResponses.Count)].Replace("USER",
                                e.User.Name));
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("/pizza", 0))
                {
                    await client.SendMessage(e.Channel, KitePizza.ParsePizza(e.User.Name, e.Message.Text));
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("sandwich", 0))
                {
                    await client.SendMessage(e.Channel, KiteSandwich.ParseSandwich(e.User.Name));
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("hi", 0) ||
                         0 <= e.Message.Text.ToLower().IndexOf("hey", 0) ||
                         0 <= e.Message.Text.ToLower().IndexOf("hello", 0))
                {
                    await client.SendMessage(e.Channel, ParseGreeting(e.User.Name));
                }
                else if (0 <= e.Message.Text.ToLower().IndexOf("/meal", 0) ||
                         0 <= e.Message.Text.ToLower().IndexOf("dinner", 0)
                         || 0 <= e.Message.Text.ToLower().IndexOf("lunch", 0))
                {
                    await
                        client.SendMessage(e.Channel,
                            _mealResponses[RandomSeed.Next(0, _mealResponses.Length)].Replace("USER",
                                e.User.Name));
                }
                else
                {
                    await
                        client.SendMessage(e.Channel, "KiteBot ver. 0.8.3 \"Less Pizza, More Meat.\"");
                }
            }
	    }

		public static string GetResponseUriFromRandomQlCrew()
		{
			string url = "http://qlcrew.com/main.php?anyone=anyone&inc%5B0%5D=&p=999&exc%5B0%5D=&per_page=15&random";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			return response.ResponseUri.AbsoluteUri;
		}
        
        //returns a greeting from the greetings.txt list on a per user or generic basis
	    private string ParseGreeting(string userName)
        {
		    if (userName.Equals("Bekenel"))
		    {
			    return (_bekGreetings[RandomSeed.Next(0, _bekGreetings.Length)]);
		    }
			else
			{
				List<string> _possibleResponses = new List<string>();

				for (int i = 0; i < _greetings.Length - 2; i += 2)
				{
					if (userName.ToLower().Contains(_greetings[i]))
					{
						_possibleResponses.Add(_greetings[i + 1]);
					}
				}

                if (_possibleResponses.Count == 0)
                {
                    for (int i = 0; i < _greetings.Length - 2; i += 2)
                    {
                        if (_greetings[i] == "generic")
                        {
                            _possibleResponses.Add(_greetings[i + 1]);
                        }
                    }
                }

				//return a random response from the context provided, replacing the string "USER" with the appropriate username
				return (_possibleResponses[RandomSeed.Next(0, _possibleResponses.Count)].Replace("USER", userName));
		    }
		    
        }

        //grabs random greetings for user bekenel from a reddit profile
		private void LoadBekGreetings()
		{
			const string url = "https://www.reddit.com/user/UWotM8_SS";
			string htmlCode;
			using (WebClient client = new WebClient())
			{
				htmlCode = client.DownloadString(url);
			}
			var regex1 = new Regex(@"<div class=""md""><p>(?<quote>.+)</p>");
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

		public void IsRaeTyping(MessageEventArgs e)
		{
			if (e.User.Name.Equals("Rae Kusoni"))
			{
				RaeCounter += -1;
			}
		}
	    public void IsRaeTyping(UserChannelEventArgs e)
	    {
			if (e.User.Name.Equals("Rae Kusoni"))
			{
				RaeCounter += 1;
			}
	    }
    }
}
