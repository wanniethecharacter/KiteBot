using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KiteBot
{
	public class DiceRoller
    {
        public static CryptoRandom Random;

        public DiceRoller()
        {
            Random = new CryptoRandom();
        }

        public string ParseRoll(string text)
        {
            Regex diceroll = new Regex(@"(?<dice>[0-9]+)d(?<sides>[0-9]+)|d?(?<single>[0-9]+)");
            var matches = diceroll.Match(text);
            int result = 0;
            try
            {
                if (matches.Groups["dice"].Success && matches.Groups["sides"].Success)
                {
                    int numberOfDice = Int32.Parse(matches.Groups["dice"].Value);
                    int numberOfSides = Int32.Parse(matches.Groups["sides"].Value);

                    if (numberOfDice > 20)
                    {
                        return "Why you do dis, too many dice.";
                    }

                    List<int> resultsHistory = new List<int>();

                    for (int i = 0; i < numberOfDice; i++)
                    {
                        resultsHistory.Add(Random.Next(1, numberOfSides));
                    }

                    string resultsString = null;
                    int counter = 0;
                    foreach (int i in resultsHistory)
                    {
                        resultsString += i.ToString();
                        result += i;

                        counter++;
                        if (counter < resultsHistory.Count)
                        {
                            resultsString += " + ";
                        }
                    }

                    resultsString += " = " + result.ToString();
                    return resultsString;
                }
                else if (matches.Groups["single"].Success)
                {
                    return Random.Next(1, Int32.Parse(matches.Groups["single"].Value)).ToString();
                }
                else
                {
                    return "use the format 5d6, d6 or simply spesify a positive integer";
                }

            }
            catch (OverflowException)
            {
                return "Why do you do this? You're on my shitlist now.";
            }
        }
    }
}
