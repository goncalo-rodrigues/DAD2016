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
        public abstract void Execute(string[] args);
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
            master?.getLogger()?.Notify((new Record($"{name} {toNotify}", DateTime.Now)));
        }
    }
}
