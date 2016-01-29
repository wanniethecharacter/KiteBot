using System.Xml.Linq;

namespace KiteBot   
{
    class Character
    {
        public string Name { get; private set; }
        public long UserId { get; private set; }
        public int Level { get; private set; }
        public int PosX { get; private set; }
        public int PosY { get; private set; }

        public Character (int xCoord, int yCoord, string name, long userId, int level)
        {
            PosX = xCoord;
            PosY = yCoord;

            Name = name;
            UserId = userId;
            Level = level;
        }

        /// <summary>
        /// Method to move a character to a new map coordinate</summary>
        /// <param name="xChange"> Change in x coordinate</param>
        /// <param name="yChange"> Change in y coordinate</param>
        /// <param name="tmpElement"> XML element reference for this character's saved data to write new map coordinates</param>

        public void Move(int xChange, int yChange, XElement tmpElement)
        {
            PosX += xChange;
            PosY += yChange;

            tmpElement.Element("x").Value = PosX.ToString();
            tmpElement.Element("y").Value = PosY.ToString();
        }

    }
}
