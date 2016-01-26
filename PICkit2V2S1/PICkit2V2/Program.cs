using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace PICkit2V2
{
    static class Program
    {
        public static StreamWriter mCmdLog; //timijk:12.13.2013
        public static StreamWriter mCmdLogScripts; 

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();         // Comment out to allow solid progress bar and tan menu
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormPICkit2());

        }

    }
}