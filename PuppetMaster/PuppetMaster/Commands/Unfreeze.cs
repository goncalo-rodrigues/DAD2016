using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster.Commands
{
    class Unfreeze : ACommand
    {

        public Unfreeze(PuppetMaster master) : base(master, "Unfreeze", "Put a freeze process back to normal operation") { }

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
