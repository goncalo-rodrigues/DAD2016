using System;
using PuppetMaster;

namespace PuppetMaster
{
    class StatusCommand : ACommand
    {
        public StatusCommand(PuppetMaster master) : base(master, "Status", "Tells all operators to print their current status.") { }

        public override void execute(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: " + name);
                return; // TODO - Throw Exception ?
            }
            master.Status();
        }
    }
}
