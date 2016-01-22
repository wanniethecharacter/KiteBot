using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using Discord;

namespace KiteBot
{
    class KitCoGame
    {
        private XDocument characterData;
        private XDocument mapData;
        public static string DataDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        public static string CharacterData = DataDirectory + "\\CharacterData.xml";
        public static string MapData = DataDirectory + "\\RoomData.xml";

        static int maxWidth = 5;
        static int maxHeight = 5;

        MapCell[,] map = new MapCell[maxWidth, maxHeight];

        public KitCoGame()
        {
            characterData = XDocument.Load(CharacterData);
            mapData = XDocument.Load(MapData);

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
                        //look commands
                        if (0 <= message.Text.ToLower().IndexOf("look"))
                        {
                            foreach (string s in map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].objectDictionary.Keys)
                            {
                                if (0 <= message.Text.ToLower().IndexOf(s.ToLower() ))
                                {
                                    return e.Element("name").Value + " " + map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].objectDictionary[s];
                                }
                            }

                            foreach (string s in map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].directions.Keys)
                            {
                                if (0 <= message.Text.ToLower().IndexOf(s.ToLower()))
                                {
                                    return e.Element("name").Value + " " + map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].directions[s];
                                }
                            }

                            return e.Element("name").Value + " " + map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].roomDesc;
                        }


                        //move commands
                        else if (0 <= message.Text.ToLower().IndexOf("move"))
                        {
                            int x = Int32.Parse(e.Element("x").Value);
                            int y = Int32.Parse(e.Element("y").Value);

                            if (0 <= message.Text.ToLower().IndexOf("north"))
                            {
                                if (y + 1 >= maxHeight)
                                    return "Cannot go that way";
                                else
                                {
                                    y++;
                                    e.Element("y").Value = y.ToString();
                                    characterData.Save(CharacterData);
                                    return e.Element("name").Value + " You move to the north. " + map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].roomDesc;
                                }
                            }

                            else if (0 <= message.Text.ToLower().IndexOf("south"))
                            {
                                if (y - 1 < 0)
                                    return "Cannot go that way";
                                else
                                {
                                    y--;
                                    e.Element("y").Value = y.ToString();
                                    characterData.Save(CharacterData);
                                    return e.Element("name").Value + " You move to the south. " + map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].roomDesc;
                                }
                            }

                            else if (0 <= message.Text.ToLower().IndexOf("east"))
                            {
                                if (x + 1 >= maxWidth)
                                    return "Cannot go that way";
                                else
                                {
                                    x++;
                                    e.Element("x").Value = x.ToString();
                                    characterData.Save(CharacterData);
                                    return e.Element("name").Value + " You move to the east. " + map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].roomDesc;
                                }
                            }

                            else if (0 <= message.Text.ToLower().IndexOf("west"))
                            {
                                if (x - 1 < 0)
                                    return "Cannot go that way";
                                else
                                {
                                    x--;
                                    e.Element("x").Value = x.ToString();
                                    characterData.Save(CharacterData);
                                    return e.Element("name").Value + " You move to the west. " + map[Int32.Parse(e.Element("x").Value), Int32.Parse(e.Element("y").Value)].roomDesc;
                                }
                            }
                        }

                        else
                            return "Welcome Back " + e.Element("name").Value + ". Current commands are look (object or direction) and move(direction)";
                    }
                }

                XElement newCharacter = new XElement("character",
                                                       new XElement("userid", message.User.Id),
                                                       new XElement("name", message.User.Name),
                                                       new XElement("level", 0),
                                                       new XElement("x", 2),
                                                       new XElement("y", 0));
                characterData.Element("chardata").Add(newCharacter);
                characterData.Save(CharacterData);

                return "Hi there, I haven't seen you before. Added new user: " + newCharacter.Element("name").Value.ToString();
            }

            else return null;
        }

    }
}
