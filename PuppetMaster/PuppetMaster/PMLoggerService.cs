﻿using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class PMLoggerService : MarshalByRefObject, ILogger
    {
        private List<Record> eventsBuffer = new List<Record>();

        public PMLoggerService()
        {
            // starts a dedicated thread that from time to time empties the buffer
            Thread t = new Thread(FlushEventBuffer);
            t.Start();
        }

        private void FlushEventBuffer()
        {
            lock (this)
            {
                while (eventsBuffer.Count == 0)
                    Monitor.Wait(this);

                // might be src of bug

                int eventsLeft = eventsBuffer.Count;
                eventsBuffer.Sort((r1, r2) => r1.CompareTo(r2));
                foreach (Record s in eventsBuffer)
                {
                    // TODO - have to change the method when we have a GUI 
                    Console.WriteLine(s.ToString());
                }
                eventsBuffer.Clear();
                Monitor.Pulse(this);
            }

            Thread.Sleep(10);
            FlushEventBuffer();
        }

        public override object InitializeLifetimeService() { return (null); }
        public void Notify(Record record)
        {
            lock (this)
            {
                eventsBuffer.Add(record);
                Monitor.Pulse(this);
            }
        }
    }
}