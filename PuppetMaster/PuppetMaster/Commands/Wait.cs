using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class WaitCommand : ACommand
    {

        public WaitCommand(PuppetMaster master) : base(master, "Wait", "Instructs the pupper master to sleep for x milliseconds") { }

        public override void Dispatch(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: " + name + "<x_ms>");
                return;
            }
            master.Wait(Int32.Parse(args[0]));
        }
    }
}
