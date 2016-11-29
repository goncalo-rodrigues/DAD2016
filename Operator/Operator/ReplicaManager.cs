using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class ReplicaManager : MarshalByRefObject, IReplica
    {

       //private List<Replica> replicas;
        public IDictionary<int, Replica> replicas;


        public ReplicaManager( Replica rep) {
            this.replicas = new Dictionary<int, Replica>();
            replicas.Add(rep.ID, rep);
           
        }

        public void AddReplica(Replica rep) {

        }

        public void Freeze(int id)
        {
            replicas[0].Freeze();
        }

        public void Interval(int id, int mils)
        {
            replicas[0].Interval(mils);
        }

        public void Kill(int id)
        {
            replicas[0].Kill();
        }

        public void Ping()
        {
            replicas[0].Ping();
        }

        public void ProcessAndForward(CTuple tuple, int id)
        {
            replicas[0].ProcessAndForward(tuple);
        }

        public void Start(int id)
        {
            replicas[0].Start();
        }

        public void Status(int id)
        {
            replicas[0].Status();
        }

        public void Unfreeze(int id)
        {
            replicas[0].Unfreeze();
        }
    }
}
