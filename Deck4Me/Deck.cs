using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
namespace Deck4Me
{

    class Deck
    {
        public string fileName = "";
        public string name = "";
        public enum deckClassTypes
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
            unknown = 9
        };
        public int deckClass = -1;
        public ArrayList cards = new ArrayList();
        public int cardTotal = 0;

        public void setClass(String strClass)
        {
            switch (strClass)
            {
                case "Warrior":
                    deckClass = (int)deckClassTypes.Warrior;
                    break;
                case "Shaman":
                    deckClass = (int)deckClassTypes.Shaman;
                    break;
                case "Rogue":
                    deckClass = (int)deckClassTypes.Rogue;
                    break;
                case "Paladin":
                    deckClass = (int)deckClassTypes.Paladin;
                    break;
                case "Hunter":
                    deckClass = (int)deckClassTypes.Hunter;
                    break;
                case "Druid":
                    deckClass = (int)deckClassTypes.Druid;
                    break;
                case "Warlock":
                    deckClass = (int)deckClassTypes.Warlock;
                    break;
                case "Mage":
                    deckClass = (int)deckClassTypes.Mage;
                    break;
                case "Priest":
                    deckClass = (int)deckClassTypes.Priest;
                    break;
                default :
                    deckClass = (int)deckClassTypes.unknown;
                    break;
            }
        }
    }
}
