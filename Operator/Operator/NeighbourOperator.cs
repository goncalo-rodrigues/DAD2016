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
        public List<int> GarbageCollectedTupleIds;
        public List<int> SentTupleIds;
        public RoutingStrategy RoutingStrategy { get; set; }
        public bool controlFlag = false;
        private Replica master;
        private DestinationInfo info; 

        public NeighbourOperator(Replica master, DestinationInfo info, Semantic semantic) : base(master, semantic)
        {
            this.master = master;
            this.info = info; 
            CachedOutputTuples = new List<CTuple>();
            GarbageCollectedTupleIds = new List<int>(new int[info.Addresses.Count]);
            SentTupleIds = new List<int>(new int[info.Addresses.Count]);
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
            // Console.WriteLine($"NeighbourOperator: Delivering Tuple {tuple.ToString()}.");
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
                    //Console.WriteLine($"The semantic At-Least-Once hasn't been implemented yet. Please consider using at-most-once instead...");
                    break;
                case Semantic.AtMostOnce:
                    rep.ProcessAndForward(tuple, id);
                    break;
                case Semantic.ExactlyOnce:
                    //Problema: O custom escreve para ficheiros, se falha a meio volta a escrever
                    //Transações ?? custom 
                    //Na replica, antes de fazer process verificar -> ter um id por tuplo a ser processado e verifico se esse id já foi processado é só 
                    Console.WriteLine($"The semantic exaclty-Once hasn't been implemented yet. Please consider using at-most-once instead...");
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
        public override void Resend(int id, int replicaId)
        {
            List<CTuple> toDeliver = new List<CTuple>();
            lock (this)
            {
                if (CachedOutputTuples.Count == 0 || id < CachedOutputTuples[0].ID)
                {
                    // Missing tuples!!!
                    throw new TuplesNotCachedException(id, CachedOutputTuples[0].ID - 1);
                }
                var upTo = SentTupleIds[replicaId];
                for (int i = 0; i < CachedOutputTuples.Count; i++)
                {
                    if (CachedOutputTuples[i].ID > upTo) break;
                    if (CachedOutputTuples[i].ID < id) continue;
                    toDeliver.Add(CachedOutputTuples[i]);
                }
            }
            foreach (var t in toDeliver)
            {
                Deliver(t);
            }
        }

        public override void GarbageCollect(int id, int replicaId)
        {
            lock (this)
            {
                GarbageCollectedTupleIds[replicaId] = id;
                var currentMin = id;

                currentMin = Math.Min(currentMin, GarbageCollectedTupleIds.Min());
                while (CachedOutputTuples[0].ID >= currentMin)
                {
                    CachedOutputTuples.RemoveAt(0);
                }
            }

        }

        public override DestinationState GetState()
        {
            return new DestinationState
            {
                SentIds = SentTupleIds
            };
        }
        public override void LoadState(DestinationState state)
        {
            this.SentTupleIds = state.SentIds;
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

    }
}
