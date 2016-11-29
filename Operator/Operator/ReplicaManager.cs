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
            this.replicas.Add(rep.ID, rep);
           
        }

        public void AddReplica(Replica rep) {
            this.replicas.Add(rep.ID, rep);
        }

        public void Freeze(int id)
        {
            replicas[id].Freeze();
        }

        public void Interval(int id, int mils)
        {
            replicas[id].Interval(mils);
        }

        public void Kill(int id)
        {
            replicas[id].Kill();
        }

        public void Ping()
        {
            foreach (Replica rep in replicas.Values) {
                rep.Ping();
            }
            
        }

        public void ProcessAndForward(CTuple tuple, int id)
        {
            replicas[id].ProcessAndForward(tuple);
        }

        public void Start(int id)
        {
            replicas[id].Start();
        }

        public void Status(int id)
        {
            replicas[id].Status();
        }

        public void Unfreeze(int id)
        {
            replicas[id].Unfreeze();
        }
    }
}
