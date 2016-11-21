using System;
using PuppetMaster;

namespace PuppetMaster
{
    class StatusCommand : ACommand
    {
        public StatusCommand(PuppetMaster master) : base(master, "Status", "Tells all operators to print their current status.") { }

        public override void Dispatch(string[] args)
        {
            master.Status();
        }
    }
}
