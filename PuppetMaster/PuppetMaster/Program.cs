using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string config = null;
            try
            {
                config = File.ReadAllText("config.txt");
            } catch (Exception e)
            {
                Console.WriteLine($"Unable parse config file. {e.Message}. Exiting...");
                return;
            }
           
            PuppetMaster.ReadAndInitializeSystem(config);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }
    }
}
