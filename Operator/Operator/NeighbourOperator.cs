using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class NeighbourOperator 
    {
        public List<IReplica> replicas;
        public RoutingStrategy RoutingStrategy { get; set; }

        // This is called after destination receives, processes and send the tuple
        [OneWayAttribute]
        public void TupleProcessedAsyncCallBack(IAsyncResult ar)
        {
            // Might be needed in the future
            RemoteProcessAsyncDelegate del = (RemoteProcessAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        public NeighbourOperator(DestinationInfo info)
        {
            replicas = info.Addresses.Select((address) => Helper.GetStub<IReplica>(address)).ToList();
            RoutingStrategy = new PrimaryStrategy(replicas);
        }

        public void Send(CTuple tuple, Semantic semantic)
        {
            var rep = RoutingStrategy.ChooseReplica();
            switch(semantic)
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
                    Console.WriteLine($"The specified semantic ({semantic}) is not supported within our system");
                    return;
            }
            
        }

        public void Ping()
        {
            // Just need to ensure that one replica is alive
            foreach (IReplica rep in replicas)
            {
                try {
                    var task = Task.Run(() => rep.Ping());
                    if (task.Wait(TimeSpan.FromMilliseconds(10)))
                        return;
                } catch (Exception e)
                {
                    // does nothing, there might be a working replica
                }
            }
            // there are no more replicas 
            throw new NeighbourOperatorIsDeadException("Neighbour Operator has no working replicas.");
        }
    }
}
