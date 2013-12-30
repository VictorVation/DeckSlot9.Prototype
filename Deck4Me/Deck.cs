using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;
using System.Xml;
namespace Deck4Me
{

    class Deck
    {
        public string fileName = "";
        public string filePath = "";
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

        bool IsDigitsOnly(string str)
        {
            if (str.Equals("") || str == null)
            {
                return false;
            }
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        int strPositionFromLoc(int loc, string[] words)
        {
            int position = 0;
            for (int i = 0; i < loc; i++)
            {
                position++;
                position += words[i].Length ;
            }
            return position;
        }

        internal Boolean loadHearthPwnTxt(string hearthpwndecklist)
        {
            int cardNumber = 0;
            int secondaryQty = 0;
            string cardName = "";
            string crdLineTrimed = "";
            string[] lines = hearthpwndecklist.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            try
            {
                foreach (string crdLine in lines)
                {
                    crdLineTrimed = crdLine.Trim();
                    string[] words = crdLineTrimed.Split(' ');
                    int next = 0;
                    Boolean secondaryFormat = false;
                    if (words.Length > 1)
                    {
                        cardNumber = 0;
                        cardName = "";
                        cardNumber = Convert.ToInt32(words[next].ToLower().Replace("x", ""));
                        next++;
                        if (IsDigitsOnly(words[next].ToLower().Replace("x", "")))
                        {
                            secondaryQty = Convert.ToInt32(words[next].ToLower().Replace("x", ""));
                            secondaryFormat = true;
                            next++;

                        }
                        if (words[next].ToLower().Equals("x"))
                        {
                            next++;
                            if (IsDigitsOnly(words[next].ToLower().Replace("x", "")))
                            {
                                secondaryQty = Convert.ToInt32(words[next].ToLower().Replace("x", ""));
                                secondaryFormat = true;
                                next++;

                            }
                        }

                        if (crdLineTrimed.Length > words[next - 1].Length + 1)
                        {
                            cardName = crdLineTrimed.Substring(strPositionFromLoc(next, words));
                        }

                        Card newCard;
                        for (int i = 0; i < cardNumber; i++)
                        {
                            newCard = new Card();
                            newCard.name = cardName;
                            if (secondaryQty > 0)
                            {
                                newCard.isSecondaryCard = true;
                                secondaryQty--;
                            }
                            else
                            {
                                newCard.isSecondaryCard = false;
                            }
                            cards.Add(newCard);
                        }
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error parsing deck: " + e.Message);
            }
            return false;
        }

        public void ExportToDotDeck()
        {
            string xsdMarkup =
                @"<xs:schema attributeFormDefault='unqualified' elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                      <xs:element name='deck'>
                        <xs:complexType>
                          <xs:sequence>
                            <xs:element type='xs:string' name='deckName'/>
                            <xs:element type='xs:string' name='class'/>
                            <xs:element name='card' maxOccurs='unbounded' minOccurs='0'>
                              <xs:complexType>
                                <xs:sequence>
                                  <xs:element type='xs:string' name='cardName'/>
                                  <xs:element type='xs:byte' name='quantity'/>
                                  <xs:element type='xs:byte' name='secondaryQty' minOccurs='0'/>
                                </xs:sequence>
                              </xs:complexType>
                            </xs:element>
                          </xs:sequence>
                        </xs:complexType>
                      </xs:element>
                    </xs:schema>";
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdMarkup)));

            string testDeckType = Enum.GetName(typeof(deckClassTypes), deckClass);

            XDocument doc1 = new XDocument(
                    new XElement("deck",
                        new XElement("deckName", name),
                        new XElement("class", testDeckType)
                    )
                );



            foreach (Card crd in getDistinct())
            {
                doc1.Root.Add(new XElement("card",
                        new XElement("cardName", crd.name),
                        new XElement("quantity", getCardQuantity(crd)),
                        new XElement("secondaryQty", getSecondaryQuantity(crd))));
            }
    

            Console.WriteLine("Validating doc1");
            bool errors = false;
            doc1.Validate(schemas, (o, e) =>
            {
                Console.WriteLine("{0}", e.Message);
                errors = true;
            });
            Console.WriteLine("doc1 {0}", errors ? "did not validate" : "validated");

            if (!errors)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = Directory.GetParent(Application.StartupPath).Parent.FullName + @"\Sample .Deck files\";
                saveFileDialog1.Filter = "deckFile|*.Deck";
                saveFileDialog1.Title = "Save a Deck";
                saveFileDialog1.ShowDialog();

                using (System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile())
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(doc1);
                    }
                    filePath = fs.Name;
                }
            }

        }

        public int getSecondaryQuantity(Card card)
        {
            int qty = 0;
            foreach (Card crd in cards)
            {
                if (crd.name.Equals(card.name) && crd.isSecondaryCard)
                {
                    qty++;
                }
            }
            return qty;
        }
        public int getCardQuantity(Card card)
        {
            int qty = 0;
            foreach (Card crd in cards)
            {
                if (crd.name.Equals(card.name))
                {
                    qty++;
                }
            }
            return qty;
        }

        public ArrayList getDistinct()
        {
            ArrayList oneOfEach = new ArrayList();
            Boolean found = false;
            foreach (Card crd in cards)
            {
                found = false;
                foreach(Card uniqueCrd in oneOfEach)
                {
                    if (crd.name.Equals(uniqueCrd.name))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    oneOfEach.Add(crd);
                }
            }

            return oneOfEach;

        }

        public string getCardQuery(Card crd)
        {
            if (crd.name.Trim().ToLower().Equals("slam"))
            {
                return "Slam survives";
            }
            else
            {
                return crd.name;
            }
        }
    }

    



    
}
