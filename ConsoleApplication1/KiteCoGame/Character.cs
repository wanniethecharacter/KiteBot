using System.Xml.Linq;

namespace KiteBot   
{
    public struct MapCoords
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    class Character
    {
        public string Name { get; private set; }
        public long UserId { get; private set; }
        public int Level { get; private set; }

        private MapCoords mapCoords = new MapCoords();

        public Character (int xCoord, int yCoord, string name, long userId, int level)
        {
            mapCoords.x = xCoord;
            mapCoords.y = yCoord;

            Name = name;
            UserId = userId;
            Level = level;
        }

        public int PosX()
        {
            return mapCoords.x;
        }

        public int PosY()
        {
            return mapCoords.y;
        }

        /// <summary>
        /// Method to move a character to a new map coordinate</summary>
        /// <param name="xChange"> Change in x coordinate</param>
        /// <param name="yChange"> Change in y coordinate</param>
        /// <param name="tmpElement"> XML element reference for this character's saved data to write new map coordinates</param>

        public void Move(int xChange, int yChange, XElement tmpElement)
        {
            mapCoords.x += xChange;
            mapCoords.y += yChange;

            tmpElement.Element("x").Value = mapCoords.x.ToString();
            tmpElement.Element("y").Value = mapCoords.y.ToString();
        }

    }
}
