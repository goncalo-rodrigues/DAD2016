﻿using System;
using PuppetMaster;

namespace PuppetMaster
{
    class IntervalCommand : ACommand
    {
        public IntervalCommand(PuppetMaster master) : base(master, "Interval", "Tells the operator to sleep for x milliseconds between to consecutive events.") { }

        public override void execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: " + name + "<operator_id> <x_ms>");
                return; // TODO - Throw Exception ?
            }
            master.Interval(args[1], Int32.Parse(args[2]));
        }
    }
}