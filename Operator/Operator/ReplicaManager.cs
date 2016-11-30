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
        private IDictionary<int, Replica> replicas;
        private List<ReplicaState> otherReplicasStates;
        private List<string> adresses;
        private OperatorInfo info;
        private PerfectFailureDetector pfd;
        private List<IReplica> otherReplicas;
        private Dictionary<string, List<IReplica>> inputReplicas;

        public ReplicaManager( Replica rep, OperatorInfo info) {

            this.replicas = new Dictionary<int, Replica>();
            this.replicas.Add(rep.ID, rep);
            this.adresses = info.Addresses;
            this.info = info;
          
            this.otherReplicasStates = new List<ReplicaState>(); 
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                var initialState = rep.GetState();
                for (int i = 0; i < info.Addresses.Count; i++)
                {
                    otherReplicasStates.Add(initialState);
                }
            });
            this.pfd = new PerfectFailureDetector();
            this.pfd.NodeFailed += OnFail;

            var initTask = Task.Run(async () =>
            {
                this.otherReplicas =
                (await Helper.GetAllStubs<IReplica>(
                    // hack to not get his own stub
                    info.Addresses.Select((address) => (rep.SelfURL != address ? address : null)).ToList()))
                    .ToList();
               

                for (int i = 0; i < info.Addresses.Count; i++)
                {
                    if (i == rep.ID) continue;
                    
                    pfd.StartMonitoringNewNode(info.Addresses[i], otherReplicas[i]);
                }

                foreach (var op in info.InputReplicas.Keys)
                {
                    this.inputReplicas[op] = (await Helper.GetAllStubs<IReplica>(info.InputReplicas[op])).ToList();
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

        public void ProcessAndForward(CTuple tuple, string senderId, int senderReplicaId, int destinationId)
        {
            replicas[destinationId].ProcessAndForward(tuple, senderId, senderReplicaId);
        }

        public void Start(int id)
        {
            replicas[id].Start();
        }

        public void Status()
        {
            // print state of the system
            string status = "Operator: " + info.ID + ", Status: " + (replicas.Values.First().processingState == true ? "Processing" : "Not Processing");
            int repCnt = 0;
            for (int i=0; i < adresses.Count;i++)
            {
                if (replicas.ContainsKey(i)) continue;
                if (pfd.IsAlive(adresses[i]))
                {
                    repCnt += 1;
                }
            }
            status += $", Alive replicas: {replicas.Count + repCnt} (of {adresses.Count}), Recovered: {replicas.Count - 1}";
            Console.WriteLine(status);
        
        }

        public void Unfreeze(int id)
        {
            replicas[id].Unfreeze();
        }


        public void OnFail(object sender, NodeFailedEventArgs e)
        {
            Console.WriteLine($"Detected failure of {e.FailedNodeName}");
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
                        Replica r = CreateReplica(failedId);
                        
                        ReplicaState repState = otherReplicasStates[failedId]; //get the last state of crashed replica
                        
                       
                        rep.LoadState(repState);

                        break;
                    }
                }
                

            }

        }

        private Replica CreateReplica(int failedId)
        {
            ReplicaCreationInfo rci = new ReplicaCreationInfo();
            //adress of crashed replica
            rci.Address = info.Addresses[failedId];
            rci.Id = failedId;
            rci.Operator = info;
            return new Replica(rci); 
        }

        public ReplicaState GetState(int id)
        {
            return replicas[id].GetState();
        }
    }
}
