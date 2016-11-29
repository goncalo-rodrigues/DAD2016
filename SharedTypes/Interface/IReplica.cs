
using SharedTypes.PerfectFailureDetector;
using System.Runtime.Remoting.Messaging;

namespace SharedTypes
{
    public interface IReplica : IPingable
    {
        void ProcessAndForward(CTuple tuple);
        

        void Start();
        void Interval(int mils);
        void Status();
        
        [OneWayAttribute()]
        void Kill();
        void Freeze();
        void Unfreeze();

    }
}
