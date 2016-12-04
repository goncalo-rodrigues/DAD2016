using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    class NeighbourOperator : Destination
    {
        public List<IReplica> replicas;
        public List<CTuple> CachedOutputTuples;
        public List<TupleID> GarbageCollectedTupleIds;
        public List<TupleID> SentTupleIds;
        public RoutingStrategy RoutingStrategy { get; set; }
        public bool controlFlag = false;
        private Replica master;
        private DestinationInfo info; 

        public NeighbourOperator(Replica master, DestinationInfo info, Semantic semantic) : base(master, semantic)
        {
            this.master = master;
            this.info = info; 
            CachedOutputTuples = new List<CTuple>();
            GarbageCollectedTupleIds = new List<TupleID>(new TupleID[info.Addresses.Count]);
            SentTupleIds = new List<TupleID>(new TupleID[info.Addresses.Count]);
            for(int i=0; i < GarbageCollectedTupleIds.Count; i++)
            {
                GarbageCollectedTupleIds[i] = new TupleID();
                SentTupleIds[i] = new TupleID();
            }
            var replicasTask = Helper.GetAllStubs<IReplica>(info.Addresses);
            var initTask = Task.Run(async () =>
            {
                this.replicas = (await replicasTask).ToList();

                if (info.RtStrategy == SharedTypes.RoutingStrategy.Primary)
                {
                    RoutingStrategy = new PrimaryStrategy(info.Addresses.Count);
                }
                else if (info.RtStrategy == SharedTypes.RoutingStrategy.Hashing)
                {
                    RoutingStrategy = new HashingStrategy(info.Addresses.Count, info.HashingArg);
                }
                else
                {
                    RoutingStrategy = new RandomStrategy(info.Addresses.Count);
                }

            });
        }
        public override void Deliver(CTuple tuple)
        {
            int id = RoutingStrategy.ChooseReplica(tuple);
            var rep = replicas[id];
            switch (Semantic)
            {
                case Semantic.AtLeastOnce:
                    controlFlag = false;
                    
                    while (!controlFlag)
                    {
                        try
                        {
                            rep.ProcessAndForward(tuple, id);
                            controlFlag = true;
                        }
                        //FIXME Exceção que faz timeout automatico 
                        catch (Exception e) { Console.WriteLine("**********Exception"); };
                    }
                    break;
                case Semantic.AtMostOnce:
                    break;
                case Semantic.ExactlyOnce:
                    //ReplicaManager -> ProcessAndForward  
                    rep.ProcessAndForward(tuple, id);
                    break;
                default:
                    Console.WriteLine($"The specified semantic ({Semantic}) is not supported within our system");
                    break;
                
            }

            lock (this)
            {
                CachedOutputTuples.Add(tuple);
                SentTupleIds[id] = tuple.ID;
            }
        }
        public override void Resend(TupleID id, int replicaId)
        {
            List<CTuple> toDeliver = new List<CTuple>();
            lock (this)
            {

                if (id >= new TupleID(0,0) && (CachedOutputTuples.Count == 0 || id < CachedOutputTuples[0].ID))
                {
                    // Missing tuples!!!
                    Console.WriteLine($"Tuple {id} is missing");
                    throw new TuplesNotCachedException(id, CachedOutputTuples[0].ID);
                }
                var upTo = SentTupleIds[replicaId];
                for (int i = 0; i < CachedOutputTuples.Count; i++)
                {
                    if (CachedOutputTuples[i].ID > upTo) break;
                    if (CachedOutputTuples[i].ID <= id) continue;
                    toDeliver.Add(CachedOutputTuples[i]);
                }
            }
            foreach (var t in toDeliver)
            {
                Console.WriteLine($"Resending {t.ID} to {this.info.ID} ({replicaId})");
                Deliver(t);
            }
        }
        public override void GarbageCollect(TupleID id, int replicaId)
        {
            int i = 0;
            lock (this)
            {
                GarbageCollectedTupleIds[replicaId] = id;
                var garbageMin = GarbageCollectedTupleIds.Min();
                //Console.WriteLine($"GC-ing up to {id}");
                while (CachedOutputTuples.Count > 0 && garbageMin > CachedOutputTuples[0].ID)
                {
                    CachedOutputTuples.RemoveAt(0);
                    i++;
                }
            }
            if (i>0) Console.WriteLine($"GarbageCollect: Removed {i} tuples");

        }
        public override DestinationState GetState()
        {
            lock(this)
            {
                return new DestinationState
                {
                    SentIds = SentTupleIds,
                    CachedOutputTuples = CachedOutputTuples
                };
            }

        }
        public override void LoadState(DestinationState state)
        {
            this.SentTupleIds = state.SentIds;
            this.CachedOutputTuples = state.CachedOutputTuples;
        }
        public override void Ping()
        {
            // Just need to ensure that one replica is alive
            if (replicas != null && replicas.Count > 0)
                foreach (IReplica rep in replicas)
                {
                    try
                    {
                        var task = Task.Run(() => rep.Ping());
                        if (task.Wait(TimeSpan.FromMilliseconds(10)))
                            return;
                    }
                    catch (Exception e)
                    {
                        // does nothing, there might be a working replica
                    }
                }
            // there are no more replicas 
            throw new NeighbourOperatorIsDeadException("Neighbour Operator has no working replicas.");
        }

        internal override void UpdateRouting(string oldAddr, string newAddr)
        {
            bool updated = false;
            for(int i = 0; i < info?.Addresses?.Count || updated; i++)
            {
                // lets find failed replica ID
                if (info.Addresses[i].Equals(oldAddr))
                {
                    updated = true;
                    replicas[i] = Helper.GetStub<IReplica>(newAddr);
                }
            }
        }
    }
}
