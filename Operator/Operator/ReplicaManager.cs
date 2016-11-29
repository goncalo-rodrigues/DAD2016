using SharedTypes;
using SharedTypes.PerfectFailureDetector;
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
        public List<ReplicaState> otherReplicasStates;
        public List<string> adresses;


        public ReplicaManager( Replica rep) {
            this.replicas = new Dictionary<int, Replica>();
            this.replicas.Add(rep.ID, rep);
            this.adresses = rep.adresses;
            this.otherReplicasStates = new List<ReplicaState>();
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                var initialState = rep.GetState();
                for (int i = 0; i < rep.adresses.Count; i++)
                {
                    otherReplicasStates.Add(initialState);
                }
            });

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


        public void OnFail(object sender, NodeFailedEventArgs e)
        {
            int failedId = -1;
            for (int i = 0; i < this.adresses.Count; i++)
            {
                if (this.adresses[i].Equals(e.FailedNodeName))
                {
                    failedId = i;
                }
            }
            if (failedId != -1)
            {
                foreach (Replica rep in replicas.Values) {
                    if (rep.ID == failedId + 1) {
                        //recover
                        break;
                    }
                }
                

            }

        }

        public ReplicaState GetState(int id)
        {
            return replicas[id].GetState();
        }
    }
}
