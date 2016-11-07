using System;
using PuppetMaster;

namespace PuppetMaster
{
    class StatusCommand : ACommand
    {
        public StatusCommand(PuppetMaster master) : base(master, "Status", "Tells all operators to print their current status.") { }

        public override void Execute(string[] args)
        {
            if (args != null)
            {
                Console.WriteLine("Usage: " + name);
                return; // TODO - Throw Exception ?
            }
            master.Status();
            Notify(args);
        }
    }
}
