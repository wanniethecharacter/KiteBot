using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace KiteBot
{
    public class KiteChat
    {
        public static Random _randomSeed;

        public static string[] _greetings;
        public static string[] _responses;
	    public static string[] _bekGreetings;

        public static string ChatDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        public static string GreetingFileLocation = ChatDirectory + "\\Content\\Greetings.txt";
        public static string ResponseFileLocation = ChatDirectory + "\\Content\\Responses.txt";

        public KiteChat() : this(File.ReadAllLines(GreetingFileLocation), File.ReadAllLines(ResponseFileLocation), 
                                new Random(DateTime.Now.Millisecond))
        {
        }

        public KiteChat(string[] arrayOfGreetings, string[] arrayOfResponses, Random randomSeed)
        {
			LoadBekGreetings();
            _greetings = arrayOfGreetings;
            _responses = arrayOfResponses;
            _randomSeed = randomSeed;
        }

        public string ParseChatResponse(string userName, string messageText)
        {
			if (0 <= messageText.ToLower().IndexOf("fuck you", 0) || 0 <= messageText.ToLower().IndexOf("fuckyou", 0))
            {
                List<string> _possibleResponses = new List<string>();
                _possibleResponses.Add("Hey fuck you too USER!");
                _possibleResponses.Add("I bet you'd like that wouldn't you USER?");
                _possibleResponses.Add("No, fuck you USER!");
                _possibleResponses.Add("Fuck you too USER!");

                return (_possibleResponses[_randomSeed.Next(0, _possibleResponses.Count)].Replace("USER", userName));
            }
			else if (0 <= messageText.ToLower().IndexOf("/pizza", 0))
            {
                return ParsePizza(userName);
            }
			else if (0 <= messageText.ToLower().IndexOf("hi", 0) || 0 <= messageText.ToLower().IndexOf("hey", 0) ||
                0 <= messageText.ToLower().IndexOf("hello", 0))
            {
                return ParseGreeting(userName);
            }
			else
            {
                return "KiteBot ver. 0.6 \"Ask for the special sauce.\"";
            }
        }

	    private string ParseGreeting(string userName)
        {
		    if (userName.Equals("Bekenel"))
		    {
			    return (_bekGreetings[_randomSeed.Next(0, _bekGreetings.Length)]);
		    }
			else
			{
				List<string> _possibleResponses = new List<string>();

				for (int i = 0; i < _greetings.Length - 2; i += 2)
				{
					if (userName.ToLower().Contains(_greetings[i]) || _greetings[i] == "generic")
					{
						_possibleResponses.Add(_greetings[i + 1]);
					}
				}

				//return a random response from the context provided, replacing the string "USER" with the appropriate username
				return (_possibleResponses[_randomSeed.Next(0, _possibleResponses.Count)].Replace("USER", userName));
		    }
		    
        }

        private string ParsePizza(string userName)
        {
            List<string> _pizzaToppings = new List<string>();
            _pizzaToppings.AddRange (new string[] {"Extra Cheese", "Pepperoni", "Sausage", "Chicken", "Ham", "Canadian Bacon",
                                                    "Bacon", "Green Peppers", "Black Olives", "White Onion", "Red Onions", "Diced Tomatoes",
                                                    "Spinach", "Roasted Red Peppers", "Sun Dried Tomato", "Pineapple", "Italian Sausage",
                                                    "Red Onion", "Green Chile", "Basil", "Mayonnaise", "Mushrooms",});

            int _numberOfToppings = _randomSeed.Next(2, 7);//2 is 3, 7 is 8

            string _buildThisPizza = "USER you should put these things in the pizza: ";

            for (int i = 0; i <= _numberOfToppings; i++)
            {
                int j = _randomSeed.Next(0, _pizzaToppings.Count);
                _buildThisPizza += _pizzaToppings[j];
                _pizzaToppings.Remove(_pizzaToppings[j]);

                if (i == _numberOfToppings)
                {
                    _buildThisPizza += ".";
                }

                else
                {
                    _buildThisPizza += ", ";
                }
            }

            return (_buildThisPizza.Replace("USER", userName));
        }

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
    }
}
