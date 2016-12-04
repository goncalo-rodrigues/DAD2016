
using SharedTypes.PerfectFailureDetector;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace SharedTypes
{
    public interface IReplica : IPingable
    {
        void ProcessAndForward(CTuple tuple, int destinationId);
        

        void Start(int id);
        void Interval(int id, int mils);
        void Status();
        
        
        [OneWayAttribute()]
        void Kill(int id);
        void Freeze(int id);
        void Unfreeze(int id);


        void SendState(ReplicaState state, int id);
        void GarbageCollect(TupleID tupleId, string senderOpName, int senderRepId, int destinationId);
        void Resend(TupleID id, string operatorId, int replicaId, int destinationId);

        void ReRoute(string oldAddress, string newAddr);
    }
}
