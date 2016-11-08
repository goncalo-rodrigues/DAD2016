using System;
using PuppetMaster;

namespace PuppetMaster
{
    class CrashCommand : ACommand
    {
        public CrashCommand(PuppetMaster master) : base(master, "Crash", "Force a process to crash") { }

        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: " + name + "<operator_id> <rep_id>");
                return; 
            }
            master.Crash(args[0], Int32.Parse(args[1]));
            Notify(args);
        }
    }
}
