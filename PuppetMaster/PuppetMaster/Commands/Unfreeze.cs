using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class UnfreezeCommand : ACommand
    {

        public UnfreezeCommand(PuppetMaster master) : base(master, "Unfreeze", "Put a frozen process back to normal operation") { }

        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: " + name + "<operator_id> <rep_id>");
                return; 
            }
            master.Freeze(args[0], Int32.Parse(args[1]));
            Notify(args);
        }
    }
}
