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
        public static string[] _mealResponses;
        public static string[] _bekGreetings;

        public static string ChatDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
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
            _randomSeed = randomSeed;
        }

        //a method to parse chat responses not dealt with in program.cs
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
            else if (0 <= messageText.ToLower().IndexOf("/meal", 0) || 0 <= messageText.ToLower().IndexOf("dinner", 0))
            {
                return (_mealResponses[_randomSeed.Next(0, _mealResponses.Length)].Replace("USER", userName));
            }
            else
            {
                return "KiteBot ver. 0.7.1 \"Now with more stuff(ing).\"";
            }
        }

        //returns a greeting from the greetings.txt list on a per user or generic basis
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
				return (_possibleResponses[_randomSeed.Next(0, _possibleResponses.Count)].Replace("USER", userName));
		    }
		    
        }

        //Makes up and returns a list of pizza toppings, with special toppings for a specific user
        private string ParsePizza(string userName)
        {
            List<string> pizzaToppings = new List<string>();

            if (userName.ToLower().Contains("ionic"))
            {
                pizzaToppings.AddRange(new string[] {"Mayonnaise", "Squid", "Raw Tuna", "Raw Salmon", "Avocado","Squid Ink",
                                                      "Broccoli", "Shrimp", "Teriyaki Chicken", "Bonito Flakes", "Hot Sake",
                                                      "Soft Tofu", "Sushi Rice", "Nori", "Corn", "Snow Peas", "Bamboo Shoots",
                                                      "Potato", "Onion"});
            }

            else
                 pizzaToppings.AddRange (new string[] {"Extra Cheese", "Pepperoni", "Sausage", "Chicken", "Ham", "Canadian Bacon",
                                                         "Bacon", "Green Peppers", "Black Olives", "White Onion", "Red Onions", "Diced Tomatoes",
                                                         "Spinach", "Roasted Red Peppers", "Sun Dried Tomato", "Pineapple", "Italian Sausage",
                                                         "Red Onion", "Green Chile", "Basil", "Mayonnaise", "Mushrooms"});

            int numberOfToppings = _randomSeed.Next(2, 7);//2 is 3, 7 is 8

            string buildThisPizza = "USER you should put these things in the pizza: ";

            for (int i = 0; i <= numberOfToppings; i++)
            {
                int j = _randomSeed.Next(0, pizzaToppings.Count);
                buildThisPizza += pizzaToppings[j];
                pizzaToppings.Remove(pizzaToppings[j]);

                if (i == numberOfToppings)
                {
                    buildThisPizza += ".";
                }

                else
                {
                    buildThisPizza += ", ";
                }
            }

            return (buildThisPizza.Replace("USER", userName));
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
    }
}
