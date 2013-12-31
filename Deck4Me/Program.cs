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
            if (args.Length == 1) deckForm1.loadDeckWithFilePath(args[0]); 

            Application.Run(deckForm1);
        }
    }
}
