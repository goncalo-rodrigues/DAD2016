using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    class Program
    {
        public static string processName;
        public static PCServer server;
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                processName = args[0];
            }
            server = new PCServer();
            server.StartServer();
            Console.WriteLine("ProcessCreationService has started. Press Enter to exit.");
            Console.ReadLine();
            server.StopServer();
        }
    }
}
