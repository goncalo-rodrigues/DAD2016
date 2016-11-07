using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster.Commands
{
    class Freeze : ACommand
    {

        public Freeze(PuppetMaster master) : base(master, "Freeze", "Simulate a delay in the process") { }

        public override void execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: " + name + "<operator_id> <rep_id>");
                return; // TODO - Throw Exception ?
            }
            master.Freeze(args[1], Int32.Parse(args[2]));
        }
    }
}
