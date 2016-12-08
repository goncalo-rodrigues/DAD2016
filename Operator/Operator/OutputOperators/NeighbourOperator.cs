using SharedTypes;
using System;
using System.Collections.Concurrent;
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
        private List<bool> somethingSentInRecentPast;
        public RoutingStrategy RoutingStrategy { get; set; }
        public bool controlFlag = false;
        private Replica master;
        private DestinationInfo info;
        private Timer flushTimer;
       
        public NeighbourOperator(Replica master, DestinationInfo info, Semantic semantic) : base(master, semantic)
        {
            this.master = master;
            this.info = info; 
            CachedOutputTuples = new List<CTuple>();
            GarbageCollectedTupleIds = new List<TupleID>(new TupleID[info.Addresses.Count]);
            SentTupleIds = new List<TupleID>(new TupleID[info.Addresses.Count]);
            somethingSentInRecentPast = new List<bool>(new bool[info.Addresses.Count]);
            replicas = new List<IReplica>(new IReplica[info.Addresses.Count]);
            for(int i=0; i < GarbageCollectedTupleIds.Count; i++)
            {
                GarbageCollectedTupleIds[i] = new TupleID();
                SentTupleIds[i] = new TupleID();
                somethingSentInRecentPast[i] = false;
            }
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
                RoutingStrategy = new RandomStrategy(info.Addresses.Count, (master.OperatorId + info.ID + master.ID.ToString()).GetHashCode());
            }
            var replicasTask = Helper.GetAllStubs<IReplica>(info.Addresses);
            var initTask = Task.Run(async () =>
            {
                this.replicas = (await replicasTask).ToList();



            });

            flushTimer = new Timer((e) =>
            {

                Flush(master.LastSentId);
                for (int i = 0; i < replicas.Count; i++)
                    somethingSentInRecentPast[i] = false;

            }, null, 1000, 1000);
        }

        public override void Flush(TupleID flushId)
        {
            for (int i = 0; i < replicas.Count; i++)
            {
                if (!somethingSentInRecentPast[i])
                {
                    // Console.WriteLine($"Checking if need to flush. {SentTupleIds[i]} < {flushId}");
                    if (flushId >= new TupleID(0, 0) && SentTupleIds[i] < flushId)
                    {
                        //   Console.WriteLine($"Emitting flush {master.LastSentId} from {master.ID} to {i}");
                        try
                        {
                            var flushTuple = new CTuple(null, flushId.GlobalID, flushId.SubID, master.OperatorId, master.ID);
                            flushTuple.destinationId = i;
                            Insert(flushTuple);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error while flushing");
                        }
                    }
                }
            }
        }
        public override void Deliver(CTuple tuple)
        {
            // Console.WriteLine($"{GetHashCode()}: Delivering Tuple {tuple.ToString()} to {tuple.destinationId}.");
            int id = 0;
            lock(this)
            {
                id = tuple.destinationId == -1 ? RoutingStrategy.ChooseReplica(tuple) : tuple.destinationId;
                tuple.destinationId = id;
            }

            // Console.WriteLine($"{GetHashCode()}: Consulting semantic {Semantic}");

            var rep = replicas[id];
            somethingSentInRecentPast[id] = true;
            //if (tuple.GetFields() == null) //Console.WriteLine($"Delivering flush {tuple.ID} to {id}");
            switch (Semantic)
            {
                case Semantic.AtLeastOnce:
                case Semantic.ExactlyOnce:
                    controlFlag = false;
                    
                    while (!controlFlag)
                    {
                        rep = replicas[id]; //maybe someone updated it
                        try
                        {
                            rep.ProcessAndForward(tuple, id);
                            controlFlag = true;
                        }
                        //FIXME Exceção que faz timeout automatico 
                        catch (Exception e) { Console.WriteLine($"AtLeastOnce : {e.Message}"); };
                    }
                    break;
                case Semantic.AtMostOnce:
                    rep.ProcessAndForward(tuple, id);
                    break;
                default:
                    Console.WriteLine($"The specified semantic ({Semantic}) is not supported within our system");
                    break;
                
            }

            lock(this) {
                Console.WriteLine($"Delivered tuple {tuple.ID} to {info.ID} ({id})");
                CachedOutputTuples.Add(tuple);
                SentTupleIds[id] = tuple.ID;
            }
        }
        public override void Resend(TupleID id, int replicaId, string address)
        {
            List<CTuple> toDeliver = new List<CTuple>();
            var rep = Helper.GetStub<IReplica>(address);
            lock (this)
            {
                //Console.WriteLine("New destination: " + destination.ToString());
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
                    if (CachedOutputTuples[i].destinationId != replicaId) continue;
                    toDeliver.Add(CachedOutputTuples[i]);
                }
            }
            foreach (var t in toDeliver)
            {
               // Console.WriteLine($"****Resending {t.ID} to {this.info.ID} ({replicaId})");
                try
                {
                    rep.ProcessAndForward(t, replicaId);
                } catch(Exception e)
                {
                    Console.WriteLine("Error while resending: " + e.Message);
                }
                
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
                //Console.WriteLine($"Getting state, {string.Join(",", Buffer?.ToList())}");
                var outputBuffer = Buffer?.ToList();
                if (LastTakenTuple != null)
                {
                    outputBuffer.Insert(0, LastTakenTuple);
                }
                return new DestinationState
                {
                    SentIds = SentTupleIds,
                    CachedOutputTuples = CachedOutputTuples,
                    RoutingState = RoutingStrategy?.GetState(),
                    OutputBuffer = outputBuffer
                };
            }
        }

        public override void LoadState(DestinationState state)
        {
            if( state != null ) { 
                this.SentTupleIds = state.SentIds;
                this.CachedOutputTuples = state.CachedOutputTuples;
                this.RoutingStrategy.LoadState(state.RoutingState);

                //this.Buffer = new BlockingCollection<CTuple>(new ConcurrentQueue<CTuple>(), BufferSize); // start a buffer from the beginning
                foreach (CTuple tuple in state.OutputBuffer)
                {
                    Console.WriteLine($"Loadstate: inserting tuple {tuple}");
                    Insert(tuple);
                }
            }
        }

        internal override void UpdateRouting(string oldAddr, string newAddr)
        {
            for(int i = 0; i < info?.Addresses?.Count; i++)
            {
                // lets find failed replica ID
                if (info.Addresses[i].Equals(oldAddr))
                {
                    replicas[i] = Helper.GetStub<IReplica>(newAddr);
                  //  Console.WriteLine($"updating {oldAddr} to {newAddr}");
                }
            }
        }

        public override void Finish()
        {
            for (int i=0; i < replicas.Count; i++)
            {
                var flushTuple = new CTuple(null, 1000000, 0, master.OperatorId, master.ID);
                flushTuple.destinationId = i;
                Insert(flushTuple);
            }

            base.Finish();
        }
    }
}
