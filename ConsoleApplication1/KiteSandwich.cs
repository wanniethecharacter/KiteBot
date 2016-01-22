using System;
using System.Collections.Generic;

namespace KiteBot
{
    //Creates a randomized sandwich
    public class KiteSandwich
    {
        Random randomSeed = new Random(DateTime.Now.Millisecond);

        List<string> breadTypes = new List<string>();
        List<string> meatTypes = new List<string>();
        List<string> cheeseTypes = new List<string>();
        List<string> veggieTypes = new List<string>();
        List<string> dressingTypes = new List<string>();
        List<string> specialInstructions = new List<string>();
        List<string> categoryList = new List<string>();

        //constructor
        public KiteSandwich()
        {
            breadTypes.AddRange (new string[] {"White Bread", "Wheat Bread", "Rye Bread", "Multi-Grain Bread", "Hoagie Roll", "Baguette", "French Bread",
                                                "Whole Grain Tortilla Wrap", "Ciabatta", "Sour Dough Roll", "Flatbread"});
            meatTypes.AddRange(new string[] {"Capicola", "Sliced Chicken Breast", "Salami", "Pepperoni", "Roasted Turkey", "Roast Beef", "Ham", "Bacon", "Bologna", "Pastrami",
                                              "Corned Beef", "Honey Ham", "Smoked Turkey"});
            cheeseTypes.AddRange(new string[] {"Cheddar", "Mozerella", "American", "Swiss", "Havarti", "Muenster", "Gruyere", "Pepper Jack"});
            veggieTypes.AddRange(new string[] {"Lettuce", "Tomato", "Onion", "Bell Pepper", "Black Olive", "Cucumber", "Pickles", "Hot Peppers", "Spinach", "Avocado"});
            dressingTypes.AddRange(new string[] {"Mayo", "Mustard", "Oil and Vinegar", "Olive Oil", "Italian Dressing", "Home-Made Special Sauce", "Dijon", "Pesto"});
            specialInstructions.AddRange(new string[] {"Grilled",  "Toasted", "Panini Press", "Double Stack", "Triple Stack", "Foot Long", "Open Faced"});

            categoryList.AddRange(new string[] {"Bread: ", "Meat: ", "Cheese: ", "Toppings: ", "Dressing: "});
        }

        public string ParseSandwich(string userName)
        {
            var nl = Environment.NewLine;
            string builtSandwich = userName + " check out this sandwich:" + nl;

            int categoryTracker = 0;

            List<List<string>> optionLists = new List<List<string>>();

            optionLists.AddRange(new List<string>[] { breadTypes, meatTypes, cheeseTypes, veggieTypes, dressingTypes });

            foreach (List<string> currentList in optionLists)
            {
                int qty = randomSeed.Next(1, 3);

                //only 1 bread
                if (categoryTracker == 0)
                    qty = 1;

                //add 2 additional veggies
                if (categoryTracker == 3)
                {
                    qty += 2;
                }

                if (qty > 0)
                {
                    builtSandwich += categoryList[categoryTracker];

                    for (int i = 1; i <= qty; i++)
                    {
                        int rand = randomSeed.Next(0, currentList.Count);

                        //pull new random items until one not in the response is found
                        while (0 <= builtSandwich.IndexOf(currentList[rand], 0))
                        {
                            rand = randomSeed.Next(0, currentList.Count);
                        }

                        builtSandwich += currentList[rand];

                        if (i == qty)
                        {
                            builtSandwich += ".";
                        }

                        else
                        {
                            builtSandwich += ", ";
                        }
                    }

                    categoryTracker++;
                    builtSandwich += nl;
                }

                else categoryTracker++;
            }

            builtSandwich += "Special Instructions: " + specialInstructions[randomSeed.Next(0, specialInstructions.Count)];

            return builtSandwich;
        }
    }
}
