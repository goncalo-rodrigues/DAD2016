
using SharedTypes.PerfectFailureDetector;
using System.Runtime.Remoting.Messaging;

namespace SharedTypes
{
    public interface IReplica : IPingable
    {
        void ProcessAndForward(CTuple tuple, string senderId, int senderReplicaId, int destinationId);
        

        void Start(int id);
        void Interval(int id, int mils);
        void Status(int id);
        
        
        [OneWayAttribute()]
        void Kill(int id);
        void Freeze(int id);
        void Unfreeze(int id);


        ReplicaState GetState(int id);
        

    }
}
