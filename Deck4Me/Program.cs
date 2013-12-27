using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deck4Me
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 deckForm1 = new Form1();
            Application.Run(deckForm1);

            //This doesn't currently work
            //Trying to add the functionality of double clicking a .deck loads the deck into the application
            foreach (string filePath in args)
            {
                deckForm1.loadDeckWithFilePath(filePath);
            }

        }
    }
}
