using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace Operator
{
    // Delegate for calling other remote operators asynchronously
    public delegate void RemoteProcessAsyncDelegate(CTuple tuple);
    public delegate IEnumerable<IList<string>> ProcessDelegate(IList<string> tuple, ref object state, IList<object> args);

    // A delegate type for handling events from PuppetMaster
    public delegate void PuppetMasterEventHandler(object sender, EventArgs e);
    public delegate void PuppetMasterIntervalEventHandler(object sender, IntervalEventArgs e);


    public class Replica 
    {
        const int BUFFER_SIZE = 128;
        const int SWP_NOZORDER = 0x4;
        const int SWP_NOACTIVATE = 0x10;
        [DllImport("kernel32")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, int flags);

        public string OperatorId { get; }
        public int ID { get; set; }
        public string MasterURL { get; set; }
        public string SelfURL { get; set; }
        
        public int TupleCounter { get; set; } = 0;
        public int StartFrom { get; set; } = 0;

        private IDictionary<string, Destination> destinations;
        private List<string> inputFiles;
        private RoutingStrategy routingStrategy;
        public TupleID LastProcessedId { get; private set; } = new TupleID();
        public TupleID LastSentId { get; private set; } = new TupleID();



        private Dictionary<string, List<OriginOperator>> originOperators;
        private MergedInBuffer inBuffer;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();
        private bool shouldNotify = false;
        public bool processingState = false;
        public bool freezingState = false; 
        public Operation ProcessFunction { get; private set; }

        // event is raised when processing starts
        public event PuppetMasterEventHandler OnStart;
        public event PuppetMasterEventHandler OnFreeze;
        public event PuppetMasterEventHandler OnUnfreeze;
        public event PuppetMasterIntervalEventHandler OnInterval;

        public Replica(ReplicaCreationInfo rep)
        {
            // Console.WriteLine("\a");
            var info = rep.Operator;
            this.OperatorId = info.ID;
            this.MasterURL = info.MasterURL;
            this.ProcessFunction = Operations.GetOperation(info.OperatorFunction, info.OperatorFunctionArgs);
            
            
            this.shouldNotify = info.ShouldNotify;
            this.inputFiles = info.InputFiles;
            // primary is the first one in the array
            this.SelfURL = rep.Address;
            this.MasterURL = info.MasterURL;
            this.ID = rep.Id;
            this.destinations = new Dictionary<string, Destination>();

            if (info.OutputOperators == null || info.OutputOperators.Count == 0)
            {
                // it is an output operator
               //this.destinations.Add("output_file", new OutputFile(this, info.Semantic));
            } else
            {
               foreach (var dstInfo in info.OutputOperators)
                {
                    destinations.Add(dstInfo.ID, new NeighbourOperator(this, dstInfo, info.Semantic));
                }
            }

            var allOrigins = new List<OriginOperator>();
            this.originOperators = new Dictionary<string, List<OriginOperator>>();
            foreach (var op in info.InputReplicas.Keys)
            {
                this.originOperators[op] = new List<OriginOperator>();
                for (int i = 0; i < info.InputReplicas[op].Count; i++)
                {
                    this.originOperators[op].Add(new OriginOperator(op, i));
                    allOrigins.Add(originOperators[op][i]);
                }
                    
            }
            if (inputFiles!=null && inputFiles.Count > 0)
            {
                this.originOperators[this.OperatorId] = new List<OriginOperator>();
                this.originOperators[this.OperatorId].Add(new OriginOperator(this.OperatorId, 0));
                allOrigins.Add(originOperators[this.OperatorId][0]);

            }
            this.inBuffer = new MergedInBuffer(allOrigins);


                
            if (info.RtStrategy == SharedTypes.RoutingStrategy.Primary)
            {
                this.routingStrategy = new PrimaryStrategy(info.Addresses.Count);
            }
            else if (info.RtStrategy == SharedTypes.RoutingStrategy.Hashing)
            {
                this.routingStrategy = new HashingStrategy(info.Addresses.Count, info.HashingArg);
            }
            else
            {
                this.routingStrategy = new RandomStrategy(info.Addresses.Count, OperatorId.GetHashCode());
            }
            
            // Start reading from file(s)  

            Task.Run(() => mainLoop());
            
            if (shouldNotify)
                //destinations.Add(new LoggerDestination(this, info.Semantic, $"{OperatorId}({ID})", MasterURL));
                destinations.Add("puppet_master_logger", new LoggerDestination(this, info.Semantic, SelfURL, MasterURL));

                // Configure windows position

            string a = this.OperatorId;

            if (a.Equals("OP1")) { SetWindowPosition(600, 0, 400, 200); }
            if (a.Equals("OP2")) { SetWindowPosition(950, 0, 400, 200); }
            if (a.Equals("OP3")) { SetWindowPosition(600, 200, 400, 200); }
            if (a.Equals("OP4")) { SetWindowPosition(950, 200, 400, 200); }
            if (a.Equals("OP5")) { SetWindowPosition(600, 400, 400, 200); }
            if (a.Equals("OP6")) { SetWindowPosition(950, 400, 400, 200); }
            if (a.Equals("OP7")) { SetWindowPosition(600, 600, 400, 200); }
            if (a.Equals("OP8")) { SetWindowPosition(950, 600, 400, 200); }
            if (a.Equals("OP9")) { SetWindowPosition(600, 800, 400, 200); }
        }

        private void mainLoop()
        {
            int i = 0;
            while (true)
            {
                try
                {
                    var t = inBuffer.Next();
                    lock(this)
                    {
                        var result = Process(t);
                        foreach (var tuple in result)
                        {
                            SendToAll(tuple);
                            LastSentId = tuple.ID;
                        }
                        if (!result.Any() && t.ID.GlobalID > 0)
                        {
                            // if there are no results, update lastSentId anyways

                            var newId = new TupleID(t.ID.GlobalID - 1, 0);
                            LastSentId = LastSentId > newId ? LastSentId : newId;
                        }

                        // every 20 tuples, flush
                        if (i % 20 == 0)
                        {
                            foreach (var d in destinations.Values)
                                d.Flush(LastSentId);
                        }
                    }
                    i++;

                } catch (Exception e)
                {
                    Console.WriteLine("Replica.mainloop : " +e.Message);    
                    Thread.Sleep(10000);
                }

            }
        }

        public void StartProcessingFromFile(string path, int startFrom = 0)
        {
            try
            {
                using (var f = new StreamReader(path))
                {

                    string line = null;
                    while ((line = f.ReadLine()) != null)
                    {
                        if (line.StartsWith("%")) continue;
                        var tupleData = line.Split(',').Select((x) => x.Trim()).ToList();
             
                        var ctuple = new CTuple(tupleData, TupleCounter++,0, this.OperatorId, 0);
                        
                        Console.WriteLine($"Read tuple from file: {ctuple}");
                        
                        if (TupleCounter >= startFrom && routingStrategy.ChooseReplica(ctuple) == ID)
                        {
                            
                            ProcessAndForward(ctuple);
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to read from file {path}. Exception: {e.Message}.");
            }
        }

        private void SendToAll(CTuple tuple)
        {
             foreach (var neighbor in destinations.Values)
            {
                neighbor.Send(tuple);
            }
            
        }

        
        private IEnumerable<CTuple> Process(CTuple tuple)
        {
           
            IEnumerable<CTuple> resultTuples = null;
            // debug print 
            var data = tuple.GetFields();
            IEnumerable<IList<string>> resultData;
            var startSubId = LastProcessedId.GlobalID == tuple.ID.GlobalID ? LastProcessedId.SubID + 1 : 0;

            var origin = originOperators[tuple.opName][tuple.repID];
                

            if (origin.LastProcessedId < tuple.ID)
            {
                    
                if (data == null)
                {
                    origin.LastProcessedId = tuple.ID;
                    //this.LastProcessedId = new TupleID(tuple.ID.GlobalID, startSubId);
                    return new CTuple[0];
                } else
                {
                    resultData = ProcessFunction.Process(data);
                }
                origin.LastProcessedId = tuple.ID;
                    
            } else
            {
               // Console.WriteLine($"Already seen {tuple.ID} from {origin.OpId} ({origin.ReplicaId}). Ignoring.");
                return new CTuple[0];
            }
            resultTuples = resultData.Select((tupleData, i) => new CTuple(tupleData.ToList(), tuple.ID.GlobalID, startSubId + i, this.OperatorId, this.ID));

            if (resultTuples.Any()) this.LastProcessedId = resultTuples.Last().ID;
            
            

           // Console.WriteLine($"Processed {tuple.ToString()}");
            return resultTuples;
        }

        #region IReplica Implementation
        public void ProcessAndForward(CTuple tuple)
        {
            Console.WriteLine($"Received {tuple.ID} from {tuple.opName} ({ tuple.repID })");
            originOperators[tuple.opName][tuple.repID].Insert(tuple);
        }

        public void Start()
        {
            OnStart.Invoke(this, new EventArgs());
            processingState = true;
        }

        public void Interval(int mils)
        {
            Console.WriteLine($"Intervaling {mils}...");
            OnInterval?.Invoke(this, new IntervalEventArgs
            {
                Millis = mils
            });
        }
 
        public void Ping()
        {
           // Console.WriteLine($"{OperatorId} was pinged...");
        }
        [OneWayAttribute()] // call method without answer
        public void Kill()
        {
            Process p = System.Diagnostics.Process.GetCurrentProcess();
            //p.Dispose();
            p.Kill();
        }

        public void Flush(TupleID id, string operatorId, int repId)
        {
            ProcessAndForward(new CTuple(null, id.GlobalID, 0, operatorId, repId));
        }

        public void Freeze()
        {
            Console.WriteLine("Freezing...");
            OnFreeze?.Invoke(this, new EventArgs());
            freezingState = true;
        }

        internal void UpdateRouting(string oldAddr, string newAddr)
        {
            foreach(Destination destination in destinations?.Values)
            {
                destination.UpdateRouting(oldAddr, newAddr);
            }
        }

        public void Unfreeze()
        {
            Console.WriteLine("Unfreezing...");
            OnUnfreeze?.Invoke(this, new EventArgs());
            freezingState = false; 
        }

        public void Finish(string senderId, int replicaId)
        {
            //  if (/*todo: if all input operators have called this method */false)
            originOperators[senderId][replicaId].MarkFinish();
        }

        #endregion

        /*Just to configure windows position*/
        public static void SetWindowPosition(int x, int y, int width, int height)
        {
            SetWindowPos(Handle, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }
        public static IntPtr Handle
        {
            get
            {
                return GetConsoleWindow();
            }
        }


        public ReplicaState GetState()
        {
            //Console.WriteLine("Taking snapshot");
            lock(this)
            {
                var result = new ReplicaState();
                result.OperationInternalState = ProcessFunction.InternalState;
                result.OutputStreamsIds = new Dictionary<string, DestinationState>();
                result.InputStreamsIds = new Dictionary<string, OriginState>();

                foreach (var inputstream in originOperators)
                {
                    result.InputStreamsIds[inputstream.Key] = new OriginState
                    {
                        SentIds = inputstream.Value.Select((x) => x.LastProcessedId).ToList()
                    };
                }
                foreach (var d in destinations)
                {
                    var state = d.Value.GetState();
                    if (state != null)
                        result.OutputStreamsIds[d.Key] = state;
                }
                result.LastEmittedTuple = TupleCounter;
                result.LastProcessedId = LastProcessedId;
                result.IsStarted = processingState;
                result.IsFrozen = freezingState;
                
                return result;
            }
        }

        public void LoadState(ReplicaState state)
        {
            ProcessFunction.InternalState = state.OperationInternalState;
            foreach (var d in state.OutputStreamsIds)
            {
                destinations[d.Key].LoadState(d.Value);
            }
            foreach (var opName in state.InputStreamsIds.Keys)
            {
                var sentids = state.InputStreamsIds[opName].SentIds;
                for (int i=0; i < sentids.Count; i++)
                {
                    originOperators[opName][i].LastProcessedId = sentids[i];
                }
            }
            StartFrom = state.LastEmittedTuple;
            LastProcessedId = state.LastProcessedId;
            freezingState = state.IsFrozen;
            processingState = state.IsStarted;
            
        }

        public void Resend(TupleID id, string operatorId, int replicaId, string destination)
        {
            if (operatorId == this.OperatorId && this.ID == replicaId)
                return;

            destinations[operatorId].Resend(id, replicaId, destination);
        }

        public void GarbageCollect(TupleID id, string operatorId, int replicaId)
        {
            //Console.WriteLine("Garbage Collecting");
            destinations[operatorId].GarbageCollect(id, replicaId);
        }

        public string Status()
        {
            string result = $"{ID}: in:";
            foreach (var opName in originOperators.Keys)
            {
                result += $"<{opName}: {string.Join(",", originOperators[opName].Select((x, i) => "<" + i.ToString() + ":" + x.Status() + ">"))}>, ";
            }
            result += " out:";
            foreach (var opName in destinations.Keys)
            {
                result += $"<{opName}: {destinations[opName].Status()}>, ";
            }
            return result;
        }

        public void Init()
        {
            Task.Run(() =>
            {
                foreach (var path in inputFiles)
                {
                    StartProcessingFromFile(path, StartFrom);
                }
                var tuple = new CTuple(null, Int32.MaxValue, 0, OperatorId, 0);
                originOperators[OperatorId][0].Insert(tuple);
            });
        }
    }


    public class IntervalEventArgs : EventArgs
    {
        public int Millis { get; set; }
    }
}
