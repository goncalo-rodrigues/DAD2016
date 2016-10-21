using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class NeighbourOperator 
    {
        public List<IReplica> replicas;
        public RoutingStrategy RoutingStrategy { get; set; }

        public NeighbourOperator(DestinationInfo info)
        {
            replicas = info.Addresses.Select((address) => Helper.GetStub<IReplica>(address)).ToList();
        }

        public void Send(CTuple tuple, Semantic semantic)
        {
            RoutingStrategy.ChooseReplica(replicas).ProcessAndForward(tuple);
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
