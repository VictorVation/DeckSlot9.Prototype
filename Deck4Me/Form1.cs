using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseManipulator;
using System.IO;
using System.Drawing.Imaging;
using System.Xml.Linq;
using System.Xml;
using System.Collections;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Deck4Me
{

    public partial class Form1 : Form
    {
        ImageList deckImages = new ImageList();
        ImageList btnImages = new ImageList();
        ArrayList deckList = new ArrayList();

        List<String> filePaths = new List<string>();
        

        double speed = 1.5;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetFocus(HandleRef hWnd);

        int curX = -1;
        int curY = -1;
        int curH = -1;
        int curW = -1;

        public Form1()
        {
            InitializeComponent();

            DeckView.AllowDrop = true;


            btnImages.ColorDepth = ColorDepth.Depth32Bit;
            btnImages.ImageSize = new Size(158, 158);
            btnImages.Images.Add(Properties.Resources.EmptyButton);
            btnImages.Images.Add(Properties.Resources.chooseButton);

            LoadBtn.Image = btnImages.Images[0];
            LoadBtn.BackColor = System.Drawing.Color.Transparent;
            LoadBtn.FlatStyle = FlatStyle.Flat;
            LoadBtn.FlatAppearance.BorderSize = 0;
            LoadBtn.UseVisualStyleBackColor = true;

            DeckView.View = View.LargeIcon;


            deckImages.ImageSize = new Size(200, 79);
            deckImages.ColorDepth = ColorDepth.Depth24Bit;

            deckImages.Images.Add(Properties.Resources.WarriorDeck);
            deckImages.Images.Add(Properties.Resources.ShamanDeck);
            deckImages.Images.Add(Properties.Resources.RogueDeck);
            deckImages.Images.Add(Properties.Resources.PaladinDeck);
            deckImages.Images.Add(Properties.Resources.HunterDeck);
            deckImages.Images.Add(Properties.Resources.DruidDeck);
            deckImages.Images.Add(Properties.Resources.WarlockDeck);
            deckImages.Images.Add(Properties.Resources.MageDeck);
            deckImages.Images.Add(Properties.Resources.PriestDeck);


            DeckView.LargeImageList = deckImages;

            LoadDecks();
        }

        private void LoadDecks()
        {
            if (Properties.Settings.Default.DeckPathList == null)
            {
                Properties.Settings.Default.DeckPathList = new StringCollection();
                return;
            }

            foreach (string curFP in Properties.Settings.Default.DeckPathList)
            {
                loadDeckWithFilePath(curFP);
            }

        }


      


        private void loadDeckSlot9(Deck deckToLoad)
        {


            System.Diagnostics.Process[] hsProcesses = System.Diagnostics.Process.GetProcessesByName("Hearthstone");

            if (hsProcesses.Length > 0)
            {

                //SetFocus(new HandleRef(null, hsProcesses[0].Handle));
                SetForegroundWindow(hsProcesses[0].MainWindowHandle);
                ShowWindow(hsProcesses[0].MainWindowHandle, SW_RESTORE);
                //hsProcesses[0].MainWindowHandle.win;

                //SetForegroundWindow(handle);



                Rectangle rect = new Rectangle();
                GetWindowRect(hsProcesses[0].MainWindowHandle, out rect);

                curX = rect.X;
                curY = rect.Y;
                curW = (rect.Width - rect.X);
                curH = (rect.Height - rect.Y);

                //Bottom deck screen % H : 90.92 W : 80.6
                const double bottomDeckH = .8442;
                const double bottomDeckW = .8415;
                //VirtualMouse.MoveTo(curX + Convert.ToInt32(curW * bottomDeckW), curY + Convert.ToInt32(curW * bottomDeckH));
                //VirtualMouse.MoveTo(10000, 600);


                //Selecting bottom deck
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * bottomDeckW), curY + Convert.ToInt32(curH * bottomDeckH));
                VirtualMouse.LeftClick();

                System.Threading.Thread.Sleep(Convert.ToInt32(500 * speed));

                //Pick class
                double classH;
                double classW;

                switch (deckToLoad.deckClass)
                {
                    case (int)Deck.deckClassTypes.Warrior:
                        classH = .1778;
                        classW = .28377;
                        break;
                    case (int)Deck.deckClassTypes.Shaman:
                        classH = .33428;
                        classW = .2915;
                        break;
                    case (int)Deck.deckClassTypes.Rogue:
                        classH = .49289;
                        classW = .2915;
                        break;
                    case (int)Deck.deckClassTypes.Paladin:
                        classH = .1778;
                        classW = .4956;
                        break;
                    case (int)Deck.deckClassTypes.Hunter:
                        classH = .33428;
                        classW = .4956;
                        break;
                    case (int)Deck.deckClassTypes.Druid:
                        classH = .49289;
                        classW = .4956;
                        break;
                    case (int)Deck.deckClassTypes.Warlock:
                        classH = .1778;
                        classW = .70456;
                        break;
                    case (int)Deck.deckClassTypes.Mage:
                        classH = .33428;
                        classW = .70456;
                        break;
                    case (int)Deck.deckClassTypes.Priest:
                        classH = .49289;
                        classW = .70456;
                        break;
                    default:
                        classH = .33428;
                        classW = .2915;
                        break;
                }




                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * classH), curY + Convert.ToInt32(curH * classW));
                VirtualMouse.LeftClick();

                //Confirm class
                const double confirmClassH = .7930;
                const double confirmClassW = .8362;

                System.Threading.Thread.Sleep(Convert.ToInt32(300 * speed));
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * confirmClassH), curY + Convert.ToInt32(curH * confirmClassW));
                VirtualMouse.LeftClick();



                //Select search
                const double searchH = .4914;
                const double searchW = .9131;

                //Select card
                const double selectCardH = .1422;
                const double selectCardW = .3311;



                string[] cardNames = (string[])deckToLoad.cards.ToArray(typeof(string));

                System.Threading.Thread.Sleep(Convert.ToInt32(500 * speed));

                foreach (string curCard in cardNames)
                {
                    //query card
                    System.Threading.Thread.Sleep(Convert.ToInt32(200 * speed));
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * searchH), curY + Convert.ToInt32(curH * searchW));
                    VirtualMouse.LeftClick();
                    //Clipboard.SetText("Leeroy");
                    SendKeys.SendWait("^{A}");
                    SendKeys.SendWait(curCard);
                    SendKeys.SendWait("{Enter}");

                    System.Threading.Thread.Sleep(Convert.ToInt32(400 * speed));
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * selectCardH), curY + Convert.ToInt32(curH * selectCardW));
                    VirtualMouse.LeftClick();
                }

                //Select deck name
                const double selectDeckNameH = .8008;
                const double selectDeckNameW = .098;

                System.Threading.Thread.Sleep(Convert.ToInt32(300 * speed));
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * selectDeckNameH), curY + Convert.ToInt32(curH * selectDeckNameW));
                VirtualMouse.LeftClick();

                System.Threading.Thread.Sleep(Convert.ToInt32(200 * speed));

                SendKeys.SendWait("^{A}");
                SendKeys.SendWait(deckToLoad.name);
                SendKeys.SendWait("{Enter}");


            }
        }



        private void button2_Click_1(object sender, EventArgs e)
        {



            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();

            loadDeckWithFilePath(fd.FileName.ToString());
            
        }

        private void readXML(String xmlFile, Deck newDeck)
        {
            if (xmlFile.Contains(".Deck"))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlFile);


                    string curDeckName = doc.DocumentElement.SelectSingleNode("deckName").InnerText;
                    string curDeckClass = doc.DocumentElement.SelectSingleNode("class").InnerText;

                    newDeck.setClass(curDeckClass);
                    newDeck.name = curDeckName;

                    XmlNodeList nodes = doc.DocumentElement.SelectNodes("/deck/card");

                    int curQuantity = 0;
                    string curCardName = "";

                    foreach (XmlNode node in nodes)
                    {
                        curCardName = node.SelectSingleNode("cardName").InnerText;
                        curQuantity = Convert.ToInt32(node.SelectSingleNode("quantity").InnerText);

                        for (int i = 0; i < curQuantity; i++)
                        {
                            newDeck.cards.Add(curCardName);
                        }
                    }
                    deckList.Add(newDeck);


                }
                catch (ApplicationException ex)
                {
                    MessageBox.Show("There was a problem reading your deck. Details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (DeckView == null || DeckView.SelectedItems == null || DeckView.SelectedItems.Count == 0)
            {
                return;
            }
            ListViewItem selectedDeck = DeckView.SelectedItems[0];

            if (selectedDeck != null)
            {
                loadDeckSlot9(getDeck(selectedDeck.Text));
            }

        }

        private Deck getDeck(String fileName)
        {
            foreach (Deck curDeck in deckList)
            {
                if (curDeck.fileName.Equals(fileName))
                {
                    return curDeck;
                }
            }
            return null;
        }

        private void DeckView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DeckView == null || DeckView.SelectedItems == null || DeckView.SelectedItems.Count == 0)
            {
                LoadBtn.Image = btnImages.Images[0];
                LoadBtn.BackColor = System.Drawing.Color.Transparent;
                LoadBtn.FlatStyle = FlatStyle.Flat;
                LoadBtn.FlatAppearance.BorderSize = 0;
                return;
            }
            ListViewItem selectedDeck = DeckView.SelectedItems[0];

            if (selectedDeck != null)
            {
                LoadBtn.Image = btnImages.Images[1];
                LoadBtn.BackColor = System.Drawing.Color.Transparent;
                LoadBtn.FlatStyle = FlatStyle.Flat;
                LoadBtn.FlatAppearance.BorderSize = 0;
            }


        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {



            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();

            loadDeckWithFilePath(fd.FileName.ToString());
        }
        //very slow
        private void slowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speed = 4;
        }
        //slow
        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speed = 2;
        }
        //actually normal
        private void fastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speed = 1.5;
        }
        //fast
        private void fastToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            speed = 1;
        }



        private void DeckView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (ListViewItem listViewItem in DeckView.SelectedItems)
                {
                    foreach (string fp in filePaths)
                    {
                        if (listViewItem.Tag.Equals(fp))
                        {
                            filePaths.Remove(fp);
                            break;
                        }
                    }

                    listViewItem.Remove();
                }
            }
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {
            if (DeckView == null || DeckView.SelectedItems == null || DeckView.SelectedItems.Count == 0)
            {
                return;
            }
            ListViewItem selectedDeck = DeckView.SelectedItems[0];

            if (selectedDeck != null)
            {
                loadDeckSlot9(getDeck(selectedDeck.Text));
            }
        }

        private void loadDeckWithFilePath(string filePath)
        {

            ListViewItem item = new ListViewItem(Path.GetFileName(filePath));
            item.Tag = filePath;

            Deck newDeck = new Deck();
            newDeck.fileName = Path.GetFileName(filePath);
            readXML(filePath, newDeck);

            Image image;

            switch (newDeck.deckClass)
            {
                case (int)Deck.deckClassTypes.Warrior:
                    image = Properties.Resources.WarriorDeck;
                    break;
                case (int)Deck.deckClassTypes.Shaman:
                    image = Properties.Resources.ShamanDeck;
                    break;
                case (int)Deck.deckClassTypes.Rogue:
                    image = Properties.Resources.RogueDeck;
                    break;
                case (int)Deck.deckClassTypes.Paladin:
                    image = Properties.Resources.PaladinDeck;
                    break;
                case (int)Deck.deckClassTypes.Hunter:
                    image = Properties.Resources.HunterDeck;
                    break;
                case (int)Deck.deckClassTypes.Druid:
                    image = Properties.Resources.DruidDeck;
                    break;
                case (int)Deck.deckClassTypes.Warlock:
                    image = Properties.Resources.WarlockDeck;
                    break;
                case (int)Deck.deckClassTypes.Mage:
                    image = Properties.Resources.MageDeck;
                    break;
                case (int)Deck.deckClassTypes.Priest:
                    image = Properties.Resources.PriestDeck;
                    break;
                default:
                    image = Properties.Resources.ShamanDeck;
                    break;
            }

            Graphics graphics = Graphics.FromImage(image);
            Font deckNameFont = new Font("Consolas", 12, FontStyle.Bold);
            graphics.DrawString(newDeck.name, deckNameFont, Brushes.White, 20, 45);

            deckImages.Images.Add(image);

            item.ImageIndex = deckImages.Images.Count - 1;

            item.SubItems.Add(Path.GetFileName(filePath));
            DeckView.Items.Add(item);
            filePaths.Add(filePath);
        }

        private void DeckView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    loadDeckWithFilePath(filePath);

                }
            }
        }

        private void DeckView_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void DeckView_DoubleClick(object sender, EventArgs e)
        {
            if (DeckView == null || DeckView.SelectedItems == null || DeckView.SelectedItems.Count == 0)
            {
                return;
            }
            ListViewItem selectedDeck = DeckView.SelectedItems[0];

            if (selectedDeck != null)
            {
                loadDeckSlot9(getDeck(selectedDeck.Text));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (string curFP in filePaths)
            {
                Properties.Settings.Default.DeckPathList.Add(curFP);
            }
            Properties.Settings.Default.Save();
        }

       
    }
}
