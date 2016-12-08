using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    public abstract class Destination : BufferedOperator
    {
        public int Interval { get; set; } = 0; // do not equal this to -1
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

        public Destination(Replica parent, Semantic semantic) : base()
        {
            parent.OnFreeze += Parent_OnFreeze;
            parent.OnUnfreeze += Parent_OnUnfreeze;
            parent.OnStart += Parent_OnStart;
            parent.OnInterval += Parent_OnInterval;

            Semantic = semantic;
        }

        public void Send(CTuple tuple)
        {
           // Console.WriteLine($"Inserting {tuple.ID}");
            Insert(tuple);
          //  Console.WriteLine($"Inserted {tuple.ID}");
        }

        override public void DoStuff(CTuple tuple) {
            if (Interval > 0)
            {
               // Console.WriteLine("Interval!");
                Thread.Sleep(Interval);
            }
           
            lock(this)
            {
                while (!Processing || FreezeFlag)
                {
                    // Console.WriteLine("Stopped or frozen. Waiting...");
                    Monitor.Wait(this);
                     //Console.WriteLine("UnStopped or Unfrozen. Resuming...");
                }
                    
            }

            Deliver(tuple);
        }

        public virtual DestinationState GetState()
        {
           return null;
        }

        public virtual void LoadState(DestinationState state)
        {
            return ;
        }

        public virtual void Flush(TupleID id)
        {
            return;
        }

        public virtual void Resend(TupleID id, int replicaId, string destination)
        {
            Console.WriteLine("Resend not implemented.");
        }
        public virtual void GarbageCollect(TupleID id, int replicaId)
        {
            Console.WriteLine("GarbageCollect not implemented.");
        }

        public virtual void Finish()
        {
            MarkFinish();
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
        internal abstract void UpdateRouting(string oldAddr, string newAddr);

        #endregion ABSTRACT
    }


}
