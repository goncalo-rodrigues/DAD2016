using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class FreezeCommand : ACommand
    {

        public FreezeCommand(PuppetMaster master) : base(master, "Freeze", "Simulate a delay in the process") { }

        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: " + name + "<operator_id> <rep_id>");
                return; // TODO - Throw Exception ?
            }
            master.Freeze(args[0], Int32.Parse(args[1]));
            Notify(args);
        }
    }
}
