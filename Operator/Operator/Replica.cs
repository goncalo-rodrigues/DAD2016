using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Operator
{
    public delegate IEnumerable<IList<string>> ProcessDelegate(IList<string> tuple);
    class Replica : MarshalByRefObject
    {
        public string OperatorId { get; }
        private readonly ProcessDelegate processFunction;
        private IList<NeighbourOperator> destinations;
        private IList<Replica> otherReplicas;
        private bool shouldNotify = false;
        //private Semantic semantic;
        private int totalSeenTuples = 0;


        public Replica(ReplicaCreationInfo rep)
        {
            var info = rep.Operator;
            this.OperatorId = info.ID;
            this.otherReplicas = info.Addresses.Select((address) => GetStub(address)).ToList();
            this.destinations = info.OutputOperators.Select((dstInfo) => (Destination) new NeighbourOperator
            {
                replicas = dstInfo.Addresses.Select((address) => GetStub(address)).ToList()
            }).ToList();
            this.processFunction = Operations.GetOperation(info.OperatorFunction, info.OperatorFunctionArgs);
            this.shouldNotify = info.ShouldNotify;
        }
        //public Replica(IDictionary<string,object> replicaInfo)
        //{

        //}

        // This method should get the remote objects
        public Replica GetStub(string address)
        {
            throw new NotImplementedException();
        }
        private CTuple Process(CTuple tuple)
        {
            throw new NotImplementedException();
        }

        private void SendToAll(CTuple tuple)
        {
            throw new NotImplementedException();
        }

        public void ProcessAndForward(CTuple tuple)
        {
            var result = Process(tuple);
            SendToAll(result);
        }
    }
}
