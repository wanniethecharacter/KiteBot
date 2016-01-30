using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using Discord;

namespace KiteBot
{
	public class KitCoGame
    {
        private XDocument characterData;
        private XDocument mapData;
		private static string DataDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName + @"\KiteCoGame";
		private static string CharacterData = DataDirectory + @"\CharacterData.xml";
        private static string MapData = DataDirectory + @"\RoomData.xml";

        static int maxWidth = 5;
        static int maxHeight = 5;

        //holds a user ID referenceable list of characters
        static Dictionary<long, Character> characters = new Dictionary<long, Character>();

        MapCell[,] map = new MapCell[maxWidth, maxHeight];

        //constructor
        public KitCoGame()
        {
            //load saved data
            characterData = XDocument.Load(CharacterData);
            mapData = XDocument.Load(MapData);
            
            //Build character Dictionary
            List<XElement> characterDataList = new List<XElement>();
            characterDataList.AddRange(characterData.Descendants("character"));
            foreach (XElement e in characterDataList)
            {
                AddNewCharacter(e);
            }

            //Build rooms map
            List<XElement> roomList = new List<XElement>();
            roomList.AddRange(mapData.Descendants("room"));

            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    bool foundRoom = false;
                    foreach (XElement room in roomList)
                    {
                        if (room.Element("xcoord").Value == x.ToString() && room.Element("ycoord").Value == y.ToString())
                        {
                            map[x, y] = new MapCell(x, y, room);
                            foundRoom = true;
                            break;
                        }
                    }

                    if (!foundRoom)
                    {
                        map[x, y] = new MapCell(x, y);
                    }
                }
            }
        }

        //Public method for returning proper string messages saved in room data
        public string GetGameResponse(Message message)
        {
            string userIdString = message.User.Id.ToString();
            List<XElement> characterList = new List<XElement>();
            characterList.AddRange(characterData.Descendants("character"));

            XElement tmpXElement = null;

            if (!message.IsAuthor && message.Text.StartsWith("@KiteBotBeta"))
            {
                if (characters.ContainsKey(message.User.Id))
                {
                    Character currentCharacter = characters[message.User.Id];
                   
                    foreach(XElement e in characterList)
                    {
                        if (Int64.Parse(e.Element("userid").Value) == currentCharacter.UserId)
                        {
                            tmpXElement = e;
                            break;
                        }
                    }

                    //look commands
                    if (0 <= message.Text.ToLower().IndexOf("look"))
                    {
                        foreach (string s in map[currentCharacter.PosX, currentCharacter.PosY].objectDictionary.Keys)
                        {
                            if (0 <= message.Text.ToLower().IndexOf(s.ToLower()))
                            {
                                return currentCharacter.Name + " " + map[currentCharacter.PosX, currentCharacter.PosY].objectDictionary[s];
                            }
                        }

                        foreach (string s in map[currentCharacter.PosX, currentCharacter.PosY].directions.Keys)
                        {
                            if (0 <= message.Text.ToLower().IndexOf(s.ToLower()))
                            {
                                return currentCharacter.Name + " " + map[currentCharacter.PosX, currentCharacter.PosY].directions[s];
                            }
                        }

                        return currentCharacter.Name + " " + map[currentCharacter.PosX, currentCharacter.PosY].roomDesc;
                    }


                    //move commands
                    else if (0 <= message.Text.ToLower().IndexOf("move"))
                    {
                        int x = currentCharacter.PosX;
                        int y = currentCharacter.PosY;

                        if (0 <= message.Text.ToLower().IndexOf("north"))
                        {
                            if (y + 1 >= maxHeight)
                                return "Cannot go that way";
                            else
                            {
                                currentCharacter.Move(0,1, tmpXElement);
                                characterData.Save(CharacterData);
                                return currentCharacter.Name + " You move to the north. " + map[currentCharacter.PosX, currentCharacter.PosY].roomDesc;
                            }
                        }

                        else if (0 <= message.Text.ToLower().IndexOf("south"))
                        {
                            if (y - 1 < 0)
                                return "Cannot go that way";
                            else
                            {
                                currentCharacter.Move(0, -1, tmpXElement);
                                characterData.Save(CharacterData);
                                return currentCharacter.Name + " You move to the south. " + map[currentCharacter.PosX, currentCharacter.PosY].roomDesc;
                            }
                        }

                        else if (0 <= message.Text.ToLower().IndexOf("east"))
                        {
                            if (x + 1 >= maxWidth)
                                return "Cannot go that way";
                            else
                            {
                                currentCharacter.Move(1, 0, tmpXElement);
                                characterData.Save(CharacterData);
                                return currentCharacter.Name + " You move to the east. " + map[currentCharacter.PosX, currentCharacter.PosY].roomDesc;
                            }
                        }

                        else if (0 <= message.Text.ToLower().IndexOf("west"))
                        {
                            if (x - 1 < 0)
                                return "Cannot go that way";
                            else
                            {
                                currentCharacter.Move(-1, 0, tmpXElement);
                                characterData.Save(CharacterData);
                                return currentCharacter.Name + " You move to the west. " + map[currentCharacter.PosX, currentCharacter.PosY].roomDesc;
                            }
                        }
                    }

                    else
                        return "Welcome Back " + currentCharacter.Name + ". Current commands are look (object or direction) and move(direction)";
                }

                else
                {
                    XElement newCharacter = new XElement("character",
                                                           new XElement("userid", message.User.Id),
                                                           new XElement("name", message.User.Name),
                                                           new XElement("level", 0),
                                                           new XElement("x", 2),
                                                           new XElement("y", 0));
                    characterData.Element("chardata").Add(newCharacter);
                    characterData.Save(CharacterData);
                    AddNewCharacter(newCharacter);

                    return "Hi there, I haven't seen you before. Added new user: " + newCharacter.Element("name").Value.ToString();
                }

            }
            return null;
        }

        private void AddNewCharacter(XElement e)
        {
            characters.Add(Int64.Parse(e.Element("userid").Value), new Character(Int32.Parse(e.Element("x").Value),
                                                                                 Int32.Parse(e.Element("y").Value),
                                                                                 e.Element("name").Value,
                                                                                 Int64.Parse(e.Element("userid").Value),
                                                                                 Int32.Parse(e.Element("level").Value)));
        }

    }
}
