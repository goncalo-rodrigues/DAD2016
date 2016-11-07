using System;
using PuppetMaster;

namespace PuppetMaster
{
    class StartCommand : ACommand
    {
        public StartCommand(PuppetMaster master) : base(master, "Start", "Tells the operator to start processing tuples." ){ }

        public override void execute(string[] args)
        {
            if (args.Length < 1) {
                Console.WriteLine("Usage: " +  name + "<operator_id>");
                return; // TODO - Throw Exception ?
            }
            master.Start(args[0]);
        }
    }
}
