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
            parent.OnStart += (sender, args) =>
            {
                lock(this)
                {
                    Console.WriteLine("Proccessing = True");
                    Processing = true;
                    Monitor.Pulse(this);
                }
            };

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

        #region DEBUGCOMMANDS
        public void SetTimeOut(int mils)
        {
            Interval = mils;
        }
        public void Unfreeze()
        {
            lock (this)
            {
                FreezeFlag = false;
                Monitor.Pulse(this);

            }

        }
        #endregion DEBUGCOMMANDS

        #region ABSTRACT
        public abstract void Deliver(CTuple tuple);
        public abstract void Ping();
        #endregion ABSTRACT
    }
}
