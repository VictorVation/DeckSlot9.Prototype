using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deck4Me
{
    class Card
    {
        /*
        public enum minionTypes
        {
            None = 0,
            Beast = 1,
            Demon = 2,
        };

        public enum classTypes
        {
            Warrior = 0,
            Shaman = 1,
            Rogue = 2,
            Paladin = 3,
            Hunter = 4,
            Druid = 5,
            Warlock = 6,
            Mage = 7,
            Priest = 8,
            Neutral = 9,
            Unknown = 10
        };
        */
        public string name = "";
        public Boolean isSecondaryCard = false; //Used to select card # 2 in query

        //int manaCost;
        //Boolean isLegendary;
        //Boolean isMinion;
        //Boolean isSpell;
        //Boolean isGolden;

    }
}
