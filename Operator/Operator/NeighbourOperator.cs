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
    class NeighbourOperator 
    {
        public List<IReplica> replicas;
        public RoutingStrategy RoutingStrategy { get; set; }
        private List<CTuple> outBuffer;
        public int Interval { get; set; } = -1;
        public bool Processing { get; set; } = true;
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

        public NeighbourOperator(Replica parent, DestinationInfo info, Semantic semantic)
        {
            var replicasTask = Helper.GetAllStubs<IReplica>(info.Addresses);
            var initTask = Task.Run(async () =>
            {
                this.replicas = (await replicasTask).ToList();

                if (info.RtStrategy == SharedTypes.RoutingStrategy.Primary)
                {
                    RoutingStrategy = new PrimaryStrategy(replicas);
                }
                else if (info.RtStrategy == SharedTypes.RoutingStrategy.Hashing)
                {
                    RoutingStrategy = new HashingStrategy(replicas, info.HashingArg);
                }
                else
                {
                    RoutingStrategy = new RandomStrategy(replicas);
                }
                
            });

            parent.OnStart += (sender, args) =>
            {
                Processing = true;
                Console.WriteLine("Proccessing = True");
            };

            outBuffer = new List<CTuple>();
            Semantic = semantic;
            Thread t = new Thread(FlushEventBuffer);
            t.Start();
        }

        public void Deliver(CTuple tuple)
        {
            var rep = RoutingStrategy.ChooseReplica(tuple);
            Console.WriteLine($"Delivering Tuple {tuple.ToString()}.");
            switch (Semantic)
            {
                case Semantic.AtLeastOnce:
                    Console.WriteLine($"The semantic At-Least-Once hasn't been implemented yet. Please consider using at-most-once instead...");
                    break;
                case Semantic.AtMostOnce:
                    RemoteProcessAsyncDelegate remoteDel = new RemoteProcessAsyncDelegate(rep.ProcessAndForward);
                    IAsyncResult RemAr = remoteDel.BeginInvoke(tuple, TupleProcessedAsyncCallBack, null);
                    break;
                case Semantic.ExactlyOnce:
                    Console.WriteLine($"The semantic exaclty-Once hasn't been implemented yet. Please consider using at-most-once instead...");
                    break;
                default:
                    Console.WriteLine($"The specified semantic ({Semantic}) is not supported within our system");
                    return;
            }
        }
        public void Send(CTuple tuple)
        {
            lock (this)
            {
                outBuffer.Add(tuple);
                Monitor.Pulse(this);
            }
        }
        public void Ping()
        {
            // Just need to ensure that one replica is alive
            if(replicas!=null && replicas.Count > 0)
                foreach (IReplica rep in replicas)
                {
                    try
                    {
                        var task = Task.Run(() => rep.Ping());
                        if (task.Wait(TimeSpan.FromMilliseconds(10)))
                            return;
                    }
                    catch (Exception e)
                    {
                        // does nothing, there might be a working replica
                    }
                }
            // there are no more replicas 
            throw new NeighbourOperatorIsDeadException("Neighbour Operator has no working replicas.");
        }
        private void FlushEventBuffer()
        {
            lock (this)
            {
                while (outBuffer.Count == 0)
                    Monitor.Wait(this);
                
                int eventsLeft = outBuffer.Count;
                if( Processing && !FreezeFlag)
                {
                    foreach (CTuple s in outBuffer)
                    {
                        Deliver(s);
                        if (Interval != -1)
                        {
                            Thread.Sleep(Interval);
                        }
                    }
                    outBuffer.Clear();
                }
                Monitor.Pulse(this);
            }
            Thread.Sleep(10);
            FlushEventBuffer();
        }
        public void SetTimeOut(int mils)
        {
            Interval = mils;
        }
    }
}
