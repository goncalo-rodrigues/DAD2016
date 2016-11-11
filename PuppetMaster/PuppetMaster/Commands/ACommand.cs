using SharedTypes;
using System;

namespace PuppetMaster
{
    public abstract class ACommand
    {
        protected PuppetMaster master { get; set; }
        public string name { get; set; }
        public string help { get; set; }

        public ACommand(PuppetMaster master, string name) : this(master, name, "<no help>") { }
        public ACommand(PuppetMaster master, string name, string help)
        {
            this.master = master;
            this.name = name;
            this.help = help;
        }
        public void Execute(string[] args)
        {
            Dispatch(args);
            Notify(args);
        }

        public void Notify(string[] args)
        {
            string toNotify = "";
            if (args != null)
            {
                foreach (String arg in args)
                {
                    toNotify += arg + " ";
                }
            }
            master?.getLogger()?.Notify((new Record("command", "PM", $"{name} {toNotify}", DateTime.Now)));
        }

        public abstract void Dispatch(string[] args);
    }
}
