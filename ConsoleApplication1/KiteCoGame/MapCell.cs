using System.Collections.Generic;
using System.Xml.Linq;

namespace KiteBot
{
    class MapCell
    {
        public int x = -1;
        public int y = -1;

        public string roomName = null;
        public string roomDesc = null;
        //string dictionaries used for parsing user input in a given location.
        public Dictionary<string, string> objectDictionary = new Dictionary<string, string>();
        public Dictionary<string, string> directions = new Dictionary<string, string>();

        public MapCell(int xCoord, int yCoord, XElement roomData)
        {
            x = xCoord;
            y = yCoord;

            roomName = roomData.Element("name").Value;
            roomDesc = roomData.Element("roomdesc").Value;

            foreach (XElement roomObject in roomData.Descendants("object"))
            {
                objectDictionary.Add(roomObject.Element("objname").Value, roomObject.Element("objdesc").Value);
            }

            foreach (XElement direction in roomData.Descendants("direction"))
            {
                objectDictionary.Add(direction.Element("directionname").Value, direction.Element("direciondesc").Value);
            }
        }

        public MapCell(int xCoord, int yCoord)
        {
            x = xCoord;
            y = yCoord;
            roomName = "Empty Room";
            roomDesc = "This room is utterly devoid of interesting features.";
            objectDictionary.Add("Nothing", "An empty room stares back at you");
            directions.Add("North", "You see nothing here.");
            directions.Add("South", "You see nothing here.");
            directions.Add("East", "You see nothing here.");
            directions.Add("West", "You see nothing here.");
        }
    }
}
