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
using Deck4Me.Keyboard;
using HtmlAgilityPack;
using System.Reflection;
using System.Threading;
using Microsoft.VisualBasic;
using System.Drawing.Drawing2D;

namespace Deck4Me
{

    public partial class Form1 : Form
    {
        public enum aspectRatio
        {
            _4x3 = 0,
            _25x16 = 1,
            _16x10 = 2,
            _16x9 = 3,
            x
        };

        ImageList deckImages = new ImageList();
        ImageList btnImages = new ImageList();
        ImageList statusImages = new ImageList();
        ArrayList deckList = new ArrayList();

        List<String> filePaths = new List<string>();

        bool pauseExecution = false;
        bool haultExecution = false;

        KeyboardListener KListener = new KeyboardListener();


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

        Thread mainExecution;
        EventWaitHandle pauseControl = new EventWaitHandle(false, EventResetMode.AutoReset);

        Deck tempDeck;

        Boolean isLoading = true;

        string lastSelectedDeck = null;

        public Form1()
        {
            InitializeComponent();

            GlobalMouseHandler gmh = new GlobalMouseHandler();
            gmh.TheMouseMoved += new MouseMovedEvent(gmh_TheMouseMoved);
            Application.AddMessageFilter(gmh);

            DeckView.AllowDrop = true;
            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);

            statusImages.ColorDepth = ColorDepth.Depth16Bit;
            statusImages.ImageSize = new Size(24, 24);
            statusImages.Images.Add(Properties.Resources.statusGray);
            statusImages.Images.Add(Properties.Resources.statusYellow);
            statusImages.Images.Add(Properties.Resources.statusGreen);

            statusIcon.Image = statusImages.Images[0];

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
            DeckView.ShowItemToolTips = true;

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
            speed = Properties.Settings.Default.Speed;
            LoadDecks();
        }
        void gmh_TheMouseMoved()
        {
            

        }

        public delegate void MouseMovedEvent();

        public class GlobalMouseHandler : IMessageFilter
        {
            private const int WM_MOUSEMOVE = 0x0200;

            public event MouseMovedEvent TheMouseMoved;

            #region IMessageFilter Members

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEMOVE)
                {
                    if (TheMouseMoved != null)
                    {
                        TheMouseMoved();
                    }
                }
                // Always allow message to continue to the next filter control
                return false;
            }

            #endregion
        }



        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            Console.WriteLine(args.Key.ToString());
            switch (args.Key.ToString())
            {
                case "Space":
                    if (pauseExecution)
                    {
                        pauseExecution = false;
                        pauseControl.Set();
                        toolStripStatusLabel1.Text = "";
                    }
                    else
                    {
                        pauseExecution = true;
                        // MessageBox.Show("SPACE: Execution suspended. Press SPACE again to continue."); //too annoying
                        toolStripStatusLabel1.Text = "Execution paused. Press SPACE to resume.";
                    }
                    break;
                case "Escape":
                    haultExecution = true;
                    pauseExecution = false; //in case of pause then hault
                    break;
                case "F2":
                    if (lastSelectedDeck == null)
                    {
                        return;
                    }
                    Deck selectedDeck = getDeck(lastSelectedDeck);

                    if (selectedDeck != null)
                    {
                        mainExecution = new Thread(() => loadDeckSlot9(selectedDeck));
                        mainExecution.Start();
                    }
                    break;
                case "F5":
                    setSpeed("very slow");
                    break;
                case "F6":
                    setSpeed("slow");
                    break;
                case "F7":
                    setSpeed("normal");
                    break;
                case "F8":
                    setSpeed("fast");
                    break;
                    //Used for screen position debugging
               /* case "LeftShift":
                    //used for debugging screen percentage positions
                    Point cur_pos = Cursor.Position;
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

                        double curXPercentage = Math.Round((((double)cur_pos.X - (double)curX) / (double)curW), 6);
                        double curYPercentage = Math.Round((((double)cur_pos.Y - (double)curY) / (double)curH), 6);

                        System.Console.WriteLine(curXPercentage+" , "+curYPercentage);
                        toolStripStatusLabel1.Text = (curXPercentage + " , " + curYPercentage);
                    }
                    break;
                */

            }
        }



        private void LoadDecks()
        {
            if (Properties.Settings.Default.DeckPathList == null)
            {
                toolStripStatusLabel1.Text = "No decks found.";
                Properties.Settings.Default.DeckPathList = new StringCollection();
                isLoading = false;
                return;
            }

            foreach (string curFP in Properties.Settings.Default.DeckPathList)
            {
                loadDeckWithFilePath(curFP);
            }
            toolStripStatusLabel1.Text = "" + Properties.Settings.Default.DeckPathList.Count + " deck(s) loaded."; 

            isLoading = false;


        }





        private async void loadDeckSlot9(Deck deckToLoad)
        {

            if(pauseExecution)
            {
                pauseControl.WaitOne();
            }
            if (haultCheck())
            {
                return;
            }

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


                aspectRatio currentAspectRatio = aspectRatio.x;
                double interpretedRatio = ((double)curW / (double)curH);
                Boolean isWideScreen = false;
                
                
                if(interpretedRatio <= 1.4)
                {
                    currentAspectRatio = aspectRatio._4x3;
                }
                else if (interpretedRatio > 1.4 && interpretedRatio < 1.58)
                {
                    currentAspectRatio = aspectRatio._25x16;
                }
                else if (interpretedRatio > 1.58 && interpretedRatio < 1.66)
                {
                    currentAspectRatio = aspectRatio._16x10;
                }
                else if (interpretedRatio > 1.66)
                {
                    currentAspectRatio = aspectRatio._16x9;
                    isWideScreen = true;

                }

                //New Deck
                double bottomDeckW = .8415;
                double bottomDeckH = .8442;
                switch (currentAspectRatio)
                {
                    case aspectRatio._4x3:
                        bottomDeckW = 0.866252;
                        bottomDeckH = 0.843117;
                        break;
                    case aspectRatio._25x16:
                        bottomDeckW = 0.801993;
                        bottomDeckH = 0.84144;
                        break;
                    case aspectRatio._16x10:
                        bottomDeckW = 0.788849;
                        bottomDeckH = 0.844358;
                        break;
                    case aspectRatio._16x9:
                        bottomDeckW = 0.765439;
                        bottomDeckH = 0.846304;
                        break;
                    case aspectRatio.x:
                        bottomDeckW = .8415;
                        bottomDeckH = .8442;
                        break;
                }
                
                //VirtualMouse.MoveTo(curX + Convert.ToInt32(curW * bottomDeckW), curY + Convert.ToInt32(curW * bottomDeckH));
                //VirtualMouse.MoveTo(10000, 600);


                if (pauseExecution)
                {
                    pauseControl.WaitOne();
                }
                if (haultCheck())
                {
                    return;
                }

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
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.160187;
                                classW = 0.282389;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.216065;
                                classW = 0.282101;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.227758;
                                classW = 0.284047;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.24533;
                                classW = 0.28556;
                                break;
                            case aspectRatio.x:
                                classH = .1778;
                                classW = .28377;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Shaman:
                        classH = .33428;
                        classW = .2915;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.325039;
                                classW = 0.282389;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.353674;
                                classW = 0.282101;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.360617;
                                classW = 0.284047;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.365504;
                                classW = 0.28556;
                                break;
                            case aspectRatio.x:
                                classH = .33428;
                                classW = .2915;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Rogue:
                        classH = .49289;
                        classW = .2915;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.489114;
                                classW = 0.282389;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.491905;
                                classW = 0.282101;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.491103;
                                classW = 0.284047;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.492528;
                                classW = 0.28556;
                                break;
                            case aspectRatio.x:
                                classH = .49289;
                                classW = .2915;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Paladin:
                        classH = .1778;
                        classW = .4956;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.160187;
                                classW = 0.491903;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.216065;
                                classW = 0.4893;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.227758;
                                classW = 0.492218;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.24533;
                                classW = 0.489224;
                                break;
                            case aspectRatio.x:
                                classH = .1778;
                                classW = .4956;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Hunter:
                        classH = .33428;
                        classW = .4956;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.325039;
                                classW = 0.491903;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.353674;
                                classW = 0.4893;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.360617;
                                classW = 0.492218;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.365504;
                                classW = 0.489224;
                                break;
                            case aspectRatio.x:
                                classH = .33428;
                                classW = .4956;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Druid:
                        classH = .49289;
                        classW = .4956;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.489114;
                                classW = 0.491903;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.491905;
                                classW = 0.4893;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.491103;
                                classW = 0.492218;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.492528;
                                classW = 0.489224;
                                break;
                            case aspectRatio.x:
                                classH = .49289;
                                classW = .4956;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Warlock:
                        classH = .1778;
                        classW = .70456;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.160187;
                                classW = 0.703441;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.216065;
                                classW = 0.701362;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.227758;
                                classW = 0.702335;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.24533;
                                classW = 0.701509;
                                break;
                            case aspectRatio.x:
                                classH = .1778;
                                classW = .70456;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Mage:
                        classH = .33428;
                        classW = .70456;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.325039;
                                classW = 0.703441;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.353674;
                                classW = 0.701362;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.360617;
                                classW = 0.702335;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.365504;
                                classW = 0.701509;
                                break;
                            case aspectRatio.x:
                                classH = .33428;
                                classW = .70456;
                                break;
                        }
                        break;
                    case (int)Deck.deckClassTypes.Priest:
                        classH = .49289;
                        classW = .70456;
                        switch (currentAspectRatio)
                        {
                            case aspectRatio._4x3:
                                classH = 0.489114;
                                classW = 0.703441;
                                break;
                            case aspectRatio._25x16:
                                classH = 0.491905;
                                classW = 0.701362;
                                break;
                            case aspectRatio._16x10:
                                classH = 0.491103;
                                classW = 0.702335;
                                break;
                            case aspectRatio._16x9:
                                classH = 0.492528;
                                classW = 0.701509;
                                break;
                            case aspectRatio.x:
                                classH = .49289;
                                classW = .70456;
                                break;
                        }
                        break;
                    default:
                        classH = .33428;
                        classW = .2915;
                        break;
                }



                if (pauseExecution)
                {
                    pauseControl.WaitOne();
                }
                if (haultCheck())
                {
                    return;
                }

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * classH), curY + Convert.ToInt32(curH * classW));
                VirtualMouse.LeftClick();



                if (pauseExecution)
                {
                    pauseControl.WaitOne();
                }
                if (haultCheck())
                {
                    return;
                }

                //Confirm class
                double confirmClassH = .7930;
                double confirmClassW = .8362;
                switch (currentAspectRatio)
                {
                    case aspectRatio._4x3:
                        confirmClassH = 0.811042;
                        confirmClassW = 0.826923;
                        break;
                    case aspectRatio._25x16:
                        confirmClassH = 0.760274;
                        confirmClassW = 0.826848;
                        break;
                    case aspectRatio._16x10:
                        confirmClassH = 0.744365;
                        confirmClassW = 0.827821;
                        break;
                    case aspectRatio._16x9:
                        confirmClassH = 0.731631;
                        confirmClassW = 0.828664;
                        break;
                    case aspectRatio.x:
                        confirmClassH = .7930;
                        confirmClassW = .8362;
                        break;
                }

                System.Threading.Thread.Sleep(Convert.ToInt32(300 * speed));
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * confirmClassH), curY + Convert.ToInt32(curH * confirmClassW));
                VirtualMouse.LeftClick();



                //Select search
                double searchH = .4914;
                double searchW = .9131;

                switch (currentAspectRatio)
                {
                    case aspectRatio._4x3:
                        searchH = 0.493779;
                        searchW = 0.91498;
                        break;
                    case aspectRatio._25x16:
                        searchH = 0.496441;
                        searchW = 0.916342;
                        break;
                    case aspectRatio._16x10:
                        searchH = 0.49051;
                        searchW = 0.914397;
                        break;
                    case aspectRatio._16x9:
                        searchH = 0.496264;
                        searchW = 0.913793;
                        break;
                    case aspectRatio.x:
                        searchH = .4914;
                        searchW = .9131;
                        break;
                }


                //Select card
                double selectCardH = .1422;
                double selectCardW = .3311;
                switch (currentAspectRatio)
                {
                    case aspectRatio._4x3:
                        selectCardH = 0.120529;
                        selectCardW = 0.3583;
                        break;
                    case aspectRatio._25x16:
                        selectCardH = 0.197509;
                        selectCardW = 0.357004;
                        break;
                    case aspectRatio._16x10:
                        selectCardH = 0.198102;
                        selectCardW = 0.356031;
                        break;
                    case aspectRatio._16x9:
                        selectCardH = 0.22481;
                        selectCardW = 0.354086;
                        break;
                    case aspectRatio.x:
                        selectCardH = .1422;
                        selectCardW = .3311;
                        break;
                }

                //Select secondary card
                double selectSecondaryCardH = 0.283048;
                double selectSecondaryCardW = 0.3583;
                switch (currentAspectRatio)
                {
                    case aspectRatio._4x3:
                        selectSecondaryCardH = 0.283048;
                        selectSecondaryCardW = 0.3583;
                        break;
                    case aspectRatio._25x16:
                        selectSecondaryCardH = 0.319427;
                        selectSecondaryCardW = 0.357004;
                        break;
                    case aspectRatio._16x10:
                        selectSecondaryCardH = 0.327402;
                        selectSecondaryCardW = 0.357004;
                        break;
                    case aspectRatio._16x9:
                        selectSecondaryCardH = 0.342904;
                        selectSecondaryCardW = 0.358949;
                        break;
                    case aspectRatio.x:
                        selectSecondaryCardH = 0.283048;
                        selectSecondaryCardW = 0.3583;
                        break;
                }


                if (pauseExecution)
                {
                    pauseControl.WaitOne();
                }
                if (haultCheck())
                {
                    return;
                }

                Card[] cards = (Card[])deckToLoad.getDistinct().ToArray(typeof(Card));

                System.Threading.Thread.Sleep(Convert.ToInt32(500 * speed));

                foreach (Card curCard in cards)
                {

                    if (pauseExecution)
                    {
                        pauseControl.WaitOne();
                    }
                    if (haultCheck())
                    {
                        return;
                    }

                    //query card
                    System.Threading.Thread.Sleep(Convert.ToInt32(200 * speed));
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * searchH), curY + Convert.ToInt32(curH * searchW));
                    VirtualMouse.LeftClick();
                    //Clipboard.SetText("Leeroy");
                    SendKeys.SendWait("^{A}");
                    SendKeys.SendWait(deckToLoad.getCardQuery(curCard));
                    SendKeys.SendWait("{Enter}");
                    int secondariesToPick = deckToLoad.getSecondaryQuantity(curCard);
                    for (int i = 0; i < deckToLoad.getCardQuantity(curCard); i++)
                    {
                        System.Threading.Thread.Sleep(Convert.ToInt32(100 * speed));
                        if (secondariesToPick > 0)
                        {
                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * selectSecondaryCardH), curY + Convert.ToInt32(curH * selectSecondaryCardW));
                            secondariesToPick--;
                        }
                        else
                        {
                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * selectCardH), curY + Convert.ToInt32(curH * selectCardW));
                        }
                        
                        VirtualMouse.LeftClick();
                    }


                }



                if (pauseExecution)
                {
                    pauseControl.WaitOne();
                }
                if (haultCheck())
                {
                    return;
                }

                //Select deck name
                double selectDeckNameH = .8008;
                double selectDeckNameW = .098;
                switch (currentAspectRatio)
                {
                    case aspectRatio._4x3:
                        selectDeckNameH = 0.866252;
                        selectDeckNameW = 0.091093;
                        break;
                    case aspectRatio._25x16:
                        selectDeckNameH = 0.804483;
                        selectDeckNameW = 0.09144;
                        break;
                    case aspectRatio._16x10:
                        selectDeckNameH = 0.789442;
                        selectDeckNameW = 0.090467;
                        break;
                    case aspectRatio._16x9:
                        selectDeckNameH = 0.767064;
                        selectDeckNameW = 0.089494;
                        break;
                    case aspectRatio.x:
                        selectDeckNameH = .8008;
                        selectDeckNameW = .098;
                        break;
                }


                System.Threading.Thread.Sleep(Convert.ToInt32(300 * speed));
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(curX + Convert.ToInt32(curW * selectDeckNameH), curY + Convert.ToInt32(curH * selectDeckNameW));
                VirtualMouse.LeftClick();

                System.Threading.Thread.Sleep(Convert.ToInt32(200 * speed));

                SendKeys.SendWait("^{A}");
                SendKeys.SendWait(deckToLoad.name);
                SendKeys.SendWait("{Enter}");

            }
        }

        private Boolean haultCheck()
        {
            if (haultExecution)
            {
                haultExecution = false;
                pauseExecution = false;
                MessageBox.Show("ESC: Execution canceled.");
                //toolStripStatusLabel1.Text = "Execution haulted";
                return true;
            }
            toolStripStatusLabel1.Text = "";

            return false;
        }


        private void button2_Click_1(object sender, EventArgs e)
        {



            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();

            loadDeckWithFilePath(fd.FileName.ToString());

        }

        private void readXML(String xmlFile, Deck newDeck)
        {
            if (!File.Exists(xmlFile))
            {
                return; 
            }
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
                    int secondaryQty = 0;
                    string curCardName = "";
                    Card newCard;
                    foreach (XmlNode node in nodes)
                    {
                        secondaryQty = 0;
                        curCardName = node.SelectSingleNode("cardName").InnerText;
                        curQuantity = Convert.ToInt32(node.SelectSingleNode("quantity").InnerText);
                        if (node.SelectSingleNode("secondaryQty") != null)
                        {
                            secondaryQty = Convert.ToInt32(node.SelectSingleNode("secondaryQty").InnerText);
                        }
                        for (int i = 0; i < curQuantity; i++)
                        {
                            newCard = new Card();
                            newCard.name = curCardName;
                            if (secondaryQty > 0)
                            {
                                newCard.isSecondaryCard = true;
                                secondaryQty--;
                            }
                            else
                            {
                                newCard.isSecondaryCard = false;
                            }
                            
                            newDeck.cards.Add(newCard);
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
                mainExecution = new Thread(() => loadDeckSlot9(getDeck(selectedDeck.Text)));
                mainExecution.Start();
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
                lastSelectedDeck = null;
                return;
            }
            ListViewItem selectedDeck = DeckView.SelectedItems[0];

            if (selectedDeck != null)
            {
                LoadBtn.Image = btnImages.Images[1];
                LoadBtn.BackColor = System.Drawing.Color.Transparent;
                LoadBtn.FlatStyle = FlatStyle.Flat;
                LoadBtn.FlatAppearance.BorderSize = 0;
                lastSelectedDeck = selectedDeck.Text;
            }


        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "deckFile|*.Deck";
            fd.Title = "Import a Deck";
            fd.InitialDirectory = Directory.GetParent(Application.StartupPath).Parent.FullName +@"\Sample .Deck files\" ;
            fd.ShowDialog();

            loadDeckWithFilePath(fd.FileName.ToString());
        }

        private void setSpeed(string spd)
        {
            switch (spd)
            {
                case "very slow":
                    speed = 4;
                    if (!isLoading)
                    {
                        saveData();
                    }
                    break;
                 case "slow":
                    speed = 2;
                    if (!isLoading)
                    {
                        saveData();
                    }
                    break;
                case "normal":
                    speed = 1.5;
                    if (!isLoading)
                    {
                        saveData();
                    }
                    break;
                case "fast":
                    speed = 1;
                    if (!isLoading)
                    {
                        saveData();
                    }
                    break;

            }
        }

        //very slow
        private void slowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setSpeed("very slow");
            
        }
        //slow
        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setSpeed("slow");
        }
        //actually normal
        private void fastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setSpeed("normal");
        }
        //fast
        private void fastToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setSpeed("fast");
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
                            deckList.Remove(getDeck(fp));
                            if (!isLoading)
                            {
                                saveData();
                            }
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
                mainExecution = new Thread(() => loadDeckSlot9(getDeck(selectedDeck.Text)));
                mainExecution.Start();
            }
        }

        public void loadDeckWithFilePath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            foreach (string fps in filePaths)
            {
                if (filePath.Equals(fps))
                {
                    //file path already exists
                    return;
                }
            }

            ListViewItem item = new ListViewItem(Path.GetFileName(filePath));
            item.Tag = filePath;

            Deck newDeck = new Deck();
            newDeck.fileName = Path.GetFileName(filePath);
            newDeck.filePath = filePath;
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

            string trimedName = newDeck.name.ToString();
            if (trimedName.Length > 24)
            {
                trimedName = trimedName.Substring(0, 24);
            }

            int fontSize = 13 + Convert.ToInt32(Math.Round((24.0 / trimedName.Length) * 2.0));
            int height = 47 - Convert.ToInt32(Math.Round((24.0 / trimedName.Length) * 2.0));

            //Old method
            //Font deckNameFont = new Font("Consolas", fontSize , FontStyle.Bold);
            //graphics.DrawString(trimedName, deckNameFont, Brushes.White, 20, 45);

            // Create a GraphicsPath object.
            GraphicsPath myPath = new GraphicsPath();

            // Set up all the string parameters.
            string stringText = trimedName;
            FontFamily family = new FontFamily("Arial");
            int fontStyle = (int)FontStyle.Bold;
            int emSize = fontSize;
            Point origin = new Point(16, height);
            StringFormat format = StringFormat.GenericDefault;

            // Add the string to the path.
            myPath.AddString(stringText,
                family,
                fontStyle,
                emSize,
                origin,
                format);

            //Draw the path to the screen.
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawPath(new Pen(Brushes.Black, 4), myPath);
            graphics.FillPath(Brushes.White, myPath);

            deckImages.Images.Add(image);

            item.ImageIndex = deckImages.Images.Count - 1;
            item.ToolTipText = cardArrayToString(newDeck);

            item.SubItems.Add(Path.GetFileName(filePath));
            DeckView.Items.Add(item);
            filePaths.Add(filePath);
            if (!isLoading)
            {
                saveData();
            }
        }

        public void loadTempDeck()
        {

            if (tempDeck == null)
            {
                return;
            }

            Deck newDeck = tempDeck;
            newDeck.ExportToDotDeck();

            ListViewItem item = new ListViewItem(Path.GetFileName(newDeck.filePath));
            item.Tag = newDeck.filePath;

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

            string trimedName = newDeck.name.ToString();
            if (trimedName.Length > 24)
            {
                trimedName = trimedName.Substring(0, 24);
            }

            int fontSize = 13 + Convert.ToInt32(Math.Round((24.0 / trimedName.Length) * 2.0));
            int height = 47 - Convert.ToInt32(Math.Round((24.0 / trimedName.Length) * 2.0));

            //Old method
            //Font deckNameFont = new Font("Consolas", fontSize , FontStyle.Bold);
            //graphics.DrawString(trimedName, deckNameFont, Brushes.White, 20, 45);

            // Create a GraphicsPath object.
            GraphicsPath myPath = new GraphicsPath();

            // Set up all the string parameters.
            string stringText = trimedName;
            FontFamily family = new FontFamily("Arial");
            int fontStyle = (int)FontStyle.Bold;
            int emSize = fontSize;
            Point origin = new Point(16, height);
            StringFormat format = StringFormat.GenericDefault;

            // Add the string to the path.
            myPath.AddString(stringText,
                family,
                fontStyle,
                emSize,
                origin,
                format);

            //Draw the path to the screen.
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawPath(new Pen(Brushes.Black, 4), myPath);
            graphics.FillPath(Brushes.White, myPath);

            deckImages.Images.Add(image);

            item.ImageIndex = deckImages.Images.Count - 1;
            item.ToolTipText = cardArrayToString(newDeck);

            item.SubItems.Add(Path.GetFileName(newDeck.filePath));
            DeckView.Items.Add(item);
            filePaths.Add(newDeck.filePath);
            if (!isLoading)
            {
                saveData();
            }
            deckList.Add(newDeck);

            //Reset for more decks
            tempDeck = null;
            statusIcon.Image = statusImages.Images[0];
            toolStripComboBox1.Enabled = false;
            toolStripTextBox1.Enabled = false;
        }

        private string cardArrayToString(Deck dk)
        {
            string returnStr = "";
            foreach (Card crd in dk.getDistinct())
            {
                returnStr += dk.getCardQuantity(crd) + "x " + crd.name + "\n"; //+" "+dk.getSecondaryQuantity(crd)
            }

            return returnStr;
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
                mainExecution = new Thread(() => loadDeckSlot9(getDeck(selectedDeck.Text)));
                mainExecution.Start();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KListener.Dispose();
            if (!isLoading)
            {
                saveData();
            }
        }

        private void saveData()
        {
            Properties.Settings.Default.DeckPathList.Clear();
            Properties.Settings.Default.Speed = speed;
            foreach (string curFP in filePaths)
            {
                Properties.Settings.Default.DeckPathList.Add(curFP);
            }
            Properties.Settings.Default.Save();
            toolStripStatusLabel1.Text = "" + Properties.Settings.Default.DeckPathList.Count + " deck saved."; 
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Resources.AboutBox1 a = new Resources.AboutBox1();
            a.Show();
        }

        private void instructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {


            using (FileStream fileStream = new FileStream("Help.html", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(Properties.Resources.DeckSlot9_Help);
                }
            }
            System.Diagnostics.Process.Start("Help.html");

        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (Clipboard.ContainsText())
            {
                tempDeck = new Deck();
                if (tempDeck.loadHearthPwnTxt(Clipboard.GetText()))
                {
                    statusIcon.Image = statusImages.Images[1];
                    toolStripComboBox1.Enabled = true;
                    toolStripTextBox1.Enabled = true;
                }
                else
                {
                    statusIcon.Image = statusImages.Images[0];
                    toolStripComboBox1.Enabled = false;
                    toolStripTextBox1.Enabled = false;
                }
            }
            

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_Enter(object sender, EventArgs e)
        {
            toolStripTextBox1.Text = "";
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tempDeck != null)
            {
                tempDeck.setClass(toolStripComboBox1.Text);
                validateDeckLoad();
            }
        }

        //Old Button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please pick a class");
                return;
            }
            else if (toolStripTextBox1.Text.Equals("Deck name") || toolStripTextBox1.Text.Equals(""))
            {
                MessageBox.Show("Please enter a unique deck name");
                return;
            }
            else
            {
                if (tempDeck == null)
                {
                    return;
                }
                if (tempDeck.cards.Count == 0)
                {
                    MessageBox.Show("There are no cards in this deck.  Please check your clipboard and try again.");
                    return;
                }

                tempDeck.name = toolStripTextBox1.Text;
                loadTempDeck();
            }
        }

        //Old Button
        private void button1_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please pick a class");
                return;
            }
            else if (toolStripTextBox1.Text.Equals("Deck name") || toolStripTextBox1.Text.Equals(""))
            {
                MessageBox.Show("Please enter a unique deck name");
                return;
            }
            else
            {
                if (tempDeck == null)
                {
                    return;
                }
                if (tempDeck.cards.Count == 0)
                {
                    MessageBox.Show("There are no cards in this deck.  Please check your clipboard and try again.");
                    return;
                }

                tempDeck.name = toolStripTextBox1.Text;
                loadTempDeck();
            }
        }

        private void loadClipboardDeck()
        {
            if (toolStripComboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please pick a class");
                return;
            }
            else if (toolStripTextBox1.Text.Equals("Deck name") || toolStripTextBox1.Text.Equals(""))
            {
                MessageBox.Show("Please enter a unique deck name");
                return;
            }
            else
            {
                if (tempDeck == null)
                {
                    return;
                }
                if (tempDeck.cards.Count == 0)
                {
                    MessageBox.Show("There are no cards in this deck.  Please check your clipboard and try again.");
                    return;
                }

                tempDeck.name = toolStripTextBox1.Text;
                loadTempDeck();
            }
        }

        private void toolStripTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (validateDeckLoad() && e.KeyCode == Keys.Enter)
            {
                loadClipboardDeck();
            }
        }

        private Boolean validateDeckLoad()
        {
            if (toolStripComboBox1.SelectedIndex > -1 && toolStripTextBox1.Text.Length > 0 && !toolStripTextBox1.Text.Equals("Deck name"))
            {
                statusIcon.Image = statusImages.Images[2];
                return true;
            }
            else
            {
                statusIcon.Image = statusImages.Images[1];
                return false;
            }
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel3_Click(object sender, EventArgs e)
        {

        }
    }
}
