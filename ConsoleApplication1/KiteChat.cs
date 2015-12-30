using System;
using System.IO;
using System.Collections.Generic;

namespace KiteBot
{
    public class KiteChat
    {
        public static Random _randomSeed;

        public static string[] _greetings;
        public static string[] _responses;

        public static string ChatDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        public static string GreetingFileLocation = ChatDirectory + "\\Content\\Greetings.txt";
        public static string ResponseFileLocation = ChatDirectory + "\\Content\\Responses.txt";

        public KiteChat() : this(File.ReadAllLines(GreetingFileLocation), File.ReadAllLines(ResponseFileLocation), 
                                new Random(DateTime.Now.Millisecond))
        {
        }

        public KiteChat(string[] arrayOfGreetings, string[] arrayOfResponses, Random randomSeed)
        {
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
            List<string> _possibleResponses = new List<string>();

            for (int i = 0; i < _greetings.Length - 2; i+=2)
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
}
