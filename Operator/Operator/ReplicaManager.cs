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
        
        private string SelfURL { get; set; }
        private IDictionary<int, Replica> replicas;
        private List<ReplicaState> otherReplicasStates;
        private List<string> adresses;
        private OperatorInfo info;
        private PerfectFailureDetector pfd;
        private List<IReplica> allReplicas;
        private Dictionary<string, List<IReplica>> inputReplicas;
        private Dictionary<string, List<IReplica>> outputReplicas;
        private Timer propagateStateTimer;
        private ILogger puppetMaster;

        public ReplicaManager( Replica rep, OperatorInfo info) {
            rep.Init();
            this.SelfURL = rep.SelfURL;
            this.replicas = new Dictionary<int, Replica>();
            this.replicas.Add(rep.ID, rep);
            this.adresses = info.Addresses;
            this.info = info;
            this.inputReplicas = new Dictionary<string, List<IReplica>>();
            this.outputReplicas = new Dictionary<string, List<IReplica>>();
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

                foreach (var op in info.OutputOperators)
                {
                    this.outputReplicas[op.ID] = (await Helper.GetAllStubs<IReplica>(op.Addresses)).ToList();
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

            puppetMaster = (ILogger)Activator.GetObject(typeof(ILogger), info.MasterURL);
            Console.Title = $"{rep.OperatorId} ({rep.ID})";

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
                // my responsibility
                if (replicas.Any((x) => x.Key == (failedId+1)%adresses.Count))
                {
                    // recover all replicas
                    while (!pfd.IsAlive(adresses[failedId]))
                    {
                        Recover(failedId);
                        failedId = (failedId-1)%adresses.Count;
                    }
                } else
                // someone else gonna do it
                {
                    int replicaWhoWillRecoverFailedOne = (failedId + 1) % adresses.Count;
                    // check first non-failed replica who will attempt to recover the failed one
                    while (!pfd.IsAlive(adresses[replicaWhoWillRecoverFailedOne]))
                    {
                        replicaWhoWillRecoverFailedOne = (replicaWhoWillRecoverFailedOne + 1) % adresses.Count;
                    }
                    allReplicas[failedId] = allReplicas[replicaWhoWillRecoverFailedOne];
                }
            }

        }

        public void AddReplica(Replica rep) {
            lock(replicas)
            {
                this.replicas.Add(rep.ID, rep);
            }
           
            Console.Title += $" ({rep.ID})";
        }

        public void ProcessAndForward(CTuple tuple, int destinationId)
        {   //ExactlyOnce semantic
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
                foreach(var r in replicas)
                {
                    status += "\n\r" + r.Value.Status();
                }
                Console.WriteLine(status);
            }
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
            lock (replicas)
            {
                foreach (Replica rep in replicas.Values)
                {
                    rep.Ping();
                }
            }


        }

        public void Unfreeze(int id)
        {
            replicas[id].Unfreeze();
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

        public void Resend(TupleID id, string operatorId, int replicaId, int destinationId, string destination)
        {
            Console.WriteLine($"{operatorId} ({replicaId}) asked to resend tuples from {id}");
            replicas[destinationId].Resend(id, operatorId, replicaId, destination);
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
                    tasks.Add(Task.Run(()=>
                    {
                        try
                        {
                            rep.GarbageCollect(tupleId, thisOperatorId, id, destinationId);
                        } catch (Exception e)
                        {
                            Console.WriteLine("Error while garbage collecting.. ");
                        }
                    }));
                }
            }
            Task.WaitAll(tasks.ToArray());
        }

        public void Flush(TupleID id, string operatorId, int repId, int destinationId)
        {
            replicas[destinationId].Flush(id, operatorId, repId);
        }

        public void ReRoute(string oldAddr, string newAddr)
        {
            // update failed ReplicaManager Address in inputStream Replica Manager
            Console.WriteLine($"{oldAddr} -> {newAddr}");
            for (int i = 0; i < adresses.Count; i++)
            { 
                if (adresses[i].Equals(oldAddr))
                {
                    adresses[i] = newAddr;
                    allReplicas[i] = Helper.GetStub<IReplica>(newAddr);
                }
            }

            foreach (var opName in info.InputReplicas.Keys)
            {
                var addrs = info.InputReplicas[opName];
                for(int i=0; i < addrs.Count; i++)
                {
                    if (addrs[i].Equals(oldAddr))
                    {
                        addrs[i] = newAddr;
                        inputReplicas[opName][i] = Helper.GetStub<IReplica>(newAddr);
                    }
                }
            }
            // update replicas which were previously pointing to failed node
            foreach (Replica rep in replicas?.Values)
            {
                rep.UpdateRouting(oldAddr, newAddr);
            }

        }

        public void Recover(int failedId)
        {
            Dictionary<int, Replica> replicasCopy;
            lock (replicas)
            {
                replicasCopy = new Dictionary<int, Replica>(replicas);
            }

            Replica r = CreateReplica(failedId);
            ReplicaState repState = otherReplicasStates[failedId]; //get the last state of crashed replica
            Dictionary<string, OriginState> os = repState.InputStreamsIds;
            Console.WriteLine($"Started to recover replica {failedId} from state {repState}");
            r.LoadState(repState);
            AddReplica(r);
            allReplicas[failedId] = this;

            foreach (string opName in os.Keys)
            {

                var sentIds = os[opName].SentIds; // Only keeps the last id sent to each destination
                                                  //for each operator ask a re-sent
                if (opName == this.info.ID) continue;
                for (int j = 0; j < sentIds.Count; j++)
                {
                    while (true)
                    {
                        try
                        {
                            Console.WriteLine($"Asking for resend to {opName} ({j})");
                            inputReplicas[opName][j].Resend(sentIds[j], this.info.ID, failedId, j, SelfURL);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Resending failed. Trying again. Stay positive." + e.Message);
                        }
                    }



                }
            }

          //  Console.WriteLine("Phase 1 completed: Tuples were resent.");

            


            foreach (string opName in inputReplicas.Keys)
            {
                for (int i = 0; i < inputReplicas[opName].Count; i++)
                {
                    try
                    {
                        inputReplicas[opName][i].ReRoute(this.adresses[failedId], this.SelfURL);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ReplicaManager.Recover: Reroute of input replicas failed " + e.Message);
                    }
                
                }
            }

           // Console.WriteLine("Phase 2 completed: Input replicas were rerouted.");
            foreach (string opName in outputReplicas.Keys)
            {
                for (int i = 0; i < outputReplicas[opName].Count; i++)
                {
                    try
                    {
                        outputReplicas[opName][i].ReRoute(this.adresses[failedId], this.SelfURL);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ReplicaManager.Recover: Reroute of output replicas failed" + e.Message);
                    }
                }
            }


           // Console.WriteLine("Phase 3 completed: Output replicas were rerouted.");

            try
            {
                puppetMaster.ReRoute(this.info.ID, failedId, this.SelfURL);
            }
            catch (Exception e)
            {
                Console.WriteLine("ReplicaManager.Recover: Reroute of puppet master failed" + e.Message);
            }

            //Console.WriteLine("Phase 4 completed: Puppet master was rerouted.");


            adresses[failedId] = SelfURL;
            Console.WriteLine("MISSION COMPLETED: all recovered!");
            if (repState.IsFrozen) r.Freeze();
            if (repState.IsStarted) r.Start();
            r.Init();
            //resend 
        }
        
    }
}
