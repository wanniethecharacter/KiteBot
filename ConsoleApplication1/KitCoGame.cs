using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using Discord;

namespace KiteBot
{
    class KitCoGame
    {
        private XDocument characterData;
        public static string DataDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        public static string CharacterData = DataDirectory + "\\CharacterData.xml";

        public KitCoGame()
        {
            characterData = XDocument.Load(CharacterData);
        }

        public string GetGameResponse(Message message)
        {
            string userIdString = message.User.Id.ToString();
            List<XElement> characters = new List<XElement>();
            characters.AddRange(characterData.Descendants("character"));

            if ( !message.IsAuthor && message.Text.StartsWith("@KiteBot") )
            {
                foreach(XElement e in characters)
                {
                    if (e.Element("userid").Value == userIdString)
                    {
                        return "Welcome Back User: " + e.Element("userid").Value;
                    }
                }

                XElement newCharacter = new XElement("character",
                                                       new XElement("userid", message.User.Id),
                                                       new XElement("name", message.User.Name),
                                                       new XElement("level", 0));
                characterData.Element("chardata").Add(newCharacter);
                characterData.Save(CharacterData);

                return "Added new user: " + characterData.ToString();
            }

            else return null;
        }

    }
}
