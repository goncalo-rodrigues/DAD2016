using SharedTypes;
using SharedTypes.PerfectFailureDetector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    class ReplicaManager : MarshalByRefObject, IReplica
    {
        private const int PROPAGATE_STATE_PERIOD = 5000;

        //private List<Replica> replicas;
        private IDictionary<int, Replica> replicas;
        private List<ReplicaState> otherReplicasStates;
        private List<string> adresses;
        private OperatorInfo info;
        private PerfectFailureDetector pfd;
        private List<IReplica> allReplicas;
        private Dictionary<string, List<IReplica>> inputReplicas;
        private Timer propagateStateTimer;

        public ReplicaManager( Replica rep, OperatorInfo info) {

            this.replicas = new Dictionary<int, Replica>();
            this.replicas.Add(rep.ID, rep);
            this.adresses = info.Addresses;
            this.info = info;
            this.inputReplicas = new Dictionary<string, List<IReplica>>();
          
            this.otherReplicasStates = new List<ReplicaState>(new ReplicaState[adresses.Count]); 
                //await Task.Delay(10000);
            var initialState = rep.GetState();
            for (int i = 0; i < info.Addresses.Count; i++)
            {
                otherReplicasStates[i] = initialState;
            }
            
            this.pfd = new PerfectFailureDetector();
            this.pfd.NodeFailed += OnFail;

            var initTask = Task.Run(async () =>
            {
                this.allReplicas =
                (await Helper.GetAllStubs<IReplica>(
                    // hack to not get his own stub
                    info.Addresses.Select((address) => (rep.SelfURL != address ? address : null)).ToList()))
                    .ToList();
                allReplicas[rep.ID] = this;

                for (int i = 0; i < info.Addresses.Count; i++)
                {
                    if (i == rep.ID) continue;
                    
                    pfd.StartMonitoringNewNode(info.Addresses[i], allReplicas[i]);
                }

                foreach (var op in info.InputReplicas.Keys)
                {
                    this.inputReplicas[op] = (await Helper.GetAllStubs<IReplica>(info.InputReplicas[op])).ToList();
                }
            });

            propagateStateTimer = new Timer((e) =>
            {
                Dictionary<int, Replica> replicasCopy;
                lock(replicas)
                {
                    replicasCopy = new Dictionary<int, Replica>(replicas);
                }
                foreach(var repId in replicasCopy.Keys)
                {
                    PropagateState(repId);
                }
            }, null, PROPAGATE_STATE_PERIOD, PROPAGATE_STATE_PERIOD);

            Console.Title = $"{rep.OperatorId} ({rep.ID})";

        }

        public void AddReplica(Replica rep) {
            lock(replicas)
            {
                this.replicas.Add(rep.ID, rep);
            }
           
            Console.Title += $" ({rep.ID})";
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
            lock(replicas)
            {
                foreach (Replica rep in replicas.Values)
                {
                    rep.Ping();
                }
            }

            
        }

        public void ProcessAndForward(CTuple tuple, int destinationId)
        {
            replicas[destinationId].ProcessAndForward(tuple);
        }

        public void Start(int id)
        {
            replicas[id].Start();
        }

        public void Status()
        {
            lock (replicas)
            {
                // print state of the system
                string status = "Operator: " + info.ID + ", Status: " + (replicas.Values.First().processingState == true ? "Processing" : "Not Processing");
                int repCnt = 0;
                for (int i = 0; i < adresses.Count; i++)
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
        }

        public void Unfreeze(int id)
        {
            replicas[id].Unfreeze();
        }


        public void OnFail(object sender, NodeFailedEventArgs e)
        {
            Console.WriteLine($"Detected failure of {e.FailedNodeName}");
            int failedId = -1;
            Dictionary<int, Replica> replicasCopy;
            lock (replicas)
            {
                replicasCopy = new Dictionary<int, Replica>(replicas);
            }
            for (int i = 0; i < this.adresses.Count; i++)
            {
                if (this.adresses[i].Equals(e.FailedNodeName))
                {
                    failedId = i;
                }
            }
            if (failedId != -1)
            {

                foreach (Replica rep in replicasCopy.Values) {
                    if (rep.ID == failedId + 1) {
                        //recover 
                        Console.WriteLine($"Started to recover replica {failedId}");
                        Replica r = CreateReplica(failedId);
                        
                        ReplicaState repState = otherReplicasStates[failedId]; //get the last state of crashed replica
                        Dictionary<string, OriginState> os = repState.InputStreamsIds;
                        r.LoadState(repState);
                        foreach (string opName in os.Keys) {
                            var sentIds = os[opName].SentIds;
                            //for each operator ask a re-sent
                            for (int j = 0; j < sentIds.Count; j++)
                            {
                                //r.Resend(sentIds[j], opName, j);
                                //TODO (Telma): tens de pedir aos origins que te façam resend, nao é ao r
                            }
                        }
                        AddReplica(r);
                        allReplicas[failedId] = this;
                        
                        r.Start();
                        
                        
                        //resend 
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

        public void SendState(ReplicaState state, int id)
        {
            otherReplicasStates[id] = state;
        }

        public void GarbageCollect(TupleID tupleId, string senderOpName, int senderRepId, int destinationId)
        {
            replicas[destinationId].GarbageCollect(tupleId, senderOpName, senderRepId);
        }

        public void PropagateState(int id)
        {
            if (!replicas.ContainsKey(id)) return;
            var state = replicas[id].GetState();
            var tasks = new List<Task>();
            // send state for everyone
            foreach(var rep in allReplicas)
            {
                tasks.Add(Task.Run(() => rep.SendState(state, id)));
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            } catch (AggregateException e)
            {
                Console.WriteLine("Failed to propagate state to all replicas. " + e.Flatten().InnerException.Message);
                return;
            }
            
            // after state is safely delivered, ask every input stream to garbage collect
            tasks.Clear();
            foreach(var opName in inputReplicas.Keys)
            {
                for(int i=0;i<inputReplicas[opName].Count;i++)
                {
                    var rep = inputReplicas[opName][i];
                    var tupleId = state.InputStreamsIds[opName].SentIds[i];
                    var thisOperatorId = replicas[id].OperatorId;
                    var destinationId = i;
                    tasks.Add(Task.Run(()=>rep.GarbageCollect(tupleId, thisOperatorId, id, destinationId)));
                }
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
