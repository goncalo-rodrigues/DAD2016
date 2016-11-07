using System;
using PuppetMaster;

namespace PuppetMaster
{
    class CrashCommand : ACommand
    {
        public CrashCommand(PuppetMaster master) : base(master, "Crash", "Force a process to crash") { }

        public override void execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: " + name + "<operator_id> <rep_id>");
                return; 
            }
            master.Crash(args[1], Int32.Parse(args[2]));
        }
    }
}
