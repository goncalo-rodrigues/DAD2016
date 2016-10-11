using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    public delegate IList<string> ProcessDelegate(IList<string> tuple);
    class Replica : MarshalByRefObject
    {
        public string OperatorId { get; }
        private readonly ProcessDelegate processFunction;
        private IList<Destination> destinations;
        private IList<Replica> myReplicas;
        //private Semantic semantic;
        private int totalSeenTuples = 0;


        public Replica(string replicaInfo)
        {
        }
        //public Replica(IDictionary<string,object> replicaInfo)
        //{

        //}

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
