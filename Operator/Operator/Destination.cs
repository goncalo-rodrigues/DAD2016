using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    public abstract class Destination
    {
        private List<CTuple> outBuffer;
        public int Interval { get; set; } = -1;
        public bool Processing { get; set; } = false;
        public bool FreezeFlag { get; set; } = false;
        public Semantic Semantic { get; set; }

        // This is called after destination receives, processes and send the tuple
        [OneWayAttribute]
        public void TupleProcessedAsyncCallBack(IAsyncResult ar)
        {
            // Might be needed in the future
            RemoteProcessAsyncDelegate del = (RemoteProcessAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        public Destination(Replica parent, Semantic semantic)
        {

            parent.OnFreeze += Parent_OnFreeze;
            parent.OnUnfreeze += Parent_OnUnfreeze;
            parent.OnStart += Parent_OnStart;
            parent.OnInterval += Parent_OnInterval;

            outBuffer = new List<CTuple>();
            Semantic = semantic;
            Thread t = new Thread(FlushEventBuffer);
            t.Start();
        }

        public void Send(CTuple tuple)
        {
            lock (this)
            {
                outBuffer.Add(tuple);
                Monitor.Pulse(this);
            }
        }

        private void FlushEventBuffer()
        {
            lock (this)
            {
                while (outBuffer.Count == 0 || FreezeFlag || !Processing)
                {
                    Monitor.Wait(this);
                }
                   

                int eventsLeft = outBuffer.Count;

                foreach (CTuple s in outBuffer)
                {
                    Deliver(s);
                    if (Interval != -1)
                    {
                        Thread.Sleep(Interval);
                    }
                }
                outBuffer.Clear();

                Monitor.Pulse(this);
            }
            Thread.Sleep(10);
            FlushEventBuffer();
        }

        #region PARENTCOMMANDS
        private void Parent_OnStart(object sender, EventArgs e)
        {
            lock (this)
            {
                Processing = true;
                Monitor.Pulse(this);
            }
        }

        private void Parent_OnUnfreeze(object sender, EventArgs e)
        {
            Console.WriteLine("Unfreezing...");
            lock (this)
            {
                FreezeFlag = false;
                Monitor.Pulse(this);

            }
        }

        private void Parent_OnFreeze(object sender, EventArgs e)
        {
            Console.WriteLine("Freezing...");
            FreezeFlag = true;
        }

        private void Parent_OnInterval(object sender, IntervalEventArgs e)
        {
            Interval = e.Millis;
        }



        #endregion PARENTCOMMANDS

        #region ABSTRACT
        public abstract void Deliver(CTuple tuple);
        public abstract void Ping();
        #endregion ABSTRACT
    }
}
