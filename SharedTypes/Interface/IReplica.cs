
using System.Runtime.Remoting.Messaging;

namespace SharedTypes
{
    public interface IReplica
    {
        void ProcessAndForward(CTuple tuple);

        void Start();
        void Interval(int mils);
        void Status();
        void Ping();
        [OneWayAttribute()]
        void Kill();
        void Freeze();
        void Unfreeze();
    }
}
