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
    class NeighbourOperator : Destination
    {
        public List<IReplica> replicas;
        public RoutingStrategy RoutingStrategy { get; set; }
        public bool controlFlag = false;

        public NeighbourOperator(Replica master, DestinationInfo info, Semantic semantic) : base(master, semantic)
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
        }
        public override void Deliver(CTuple tuple)
        {
            Console.WriteLine($"Delivering Tuple {tuple.ToString()}.");
            var rep = RoutingStrategy.ChooseReplica(tuple);
            switch (Semantic)
            {
                case Semantic.AtLeastOnce:
                    controlFlag = false;
                    
                    while (!controlFlag)
                    {
                        try
                        {
                            rep.ProcessAndForward(tuple);
                            controlFlag = true;
                        }
                        catch (Exception e) { };
                    }
                    //Console.WriteLine($"The semantic At-Least-Once hasn't been implemented yet. Please consider using at-most-once instead...");
                    break;
                case Semantic.AtMostOnce:
                    rep.ProcessAndForward(tuple);
                    //RemoteProcessAsyncDelegate remoteDel = new RemoteProcessAsyncDelegate(rep.ProcessAndForward);
                    //IAsyncResult RemAr = remoteDel.BeginInvoke(tuple, TupleProcessedAsyncCallBack, null);
                    break;
                case Semantic.ExactlyOnce:
                    Console.WriteLine($"The semantic exaclty-Once hasn't been implemented yet. Please consider using at-most-once instead...");
                    break;
                default:
                    Console.WriteLine($"The specified semantic ({Semantic}) is not supported within our system");
                    return;
            }
        }
        public override void Ping()
        {
            // Just need to ensure that one replica is alive
            if (replicas != null && replicas.Count > 0)
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

    }
}
