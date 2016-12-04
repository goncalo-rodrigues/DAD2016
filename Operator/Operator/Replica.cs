﻿using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;

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

        private Dictionary<string, List<OriginOperator>> originOperators;
        private MergedInBuffer inBuffer;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();
        private bool shouldNotify = false;
        public bool processingState = false;
        public Operation ProcessFunction { get; private set; }

        // event is raised when processing starts
        public event PuppetMasterEventHandler OnStart;
        public event PuppetMasterEventHandler OnFreeze;
        public event PuppetMasterEventHandler OnUnfreeze;
        public event PuppetMasterIntervalEventHandler OnInterval;

        public Replica(ReplicaCreationInfo rep)
        {

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
               this.destinations.Add("output_file", new OutputFile(this, info.Semantic));
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
            this.OnStart += (sender, args) =>
            {
                Task.Run(() =>
                {
                    foreach (var path in inputFiles)
                    {
                        StartProcessingFromFile(path, StartFrom);
                    }
                });

            };

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
            while (true)
            {
               
                    var t = inBuffer.Next();
                    var result = Process(t);
                    foreach (var tuple in result)
                        SendToAll(tuple);
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
            lock (this)
            {
                var origin = originOperators[tuple.opName][tuple.repID];
                if (origin.LastProcessedId < tuple.ID)
                {
                    resultData = ProcessFunction.Process(data);
                    origin.LastProcessedId = tuple.ID;
                } else
                {
                    Console.WriteLine($"Already seen {tuple.ID} from {origin.OpId} ({origin.ReplicaId}). Ignoring.");
                    return new CTuple[0];
                }
            }
           
            resultTuples = resultData.Select((tupleData, i) => new CTuple(tupleData.ToList(), tuple.ID.GlobalID, i, this.OperatorId, this.ID));
            Console.WriteLine($"Processed {tuple.ToString()}");
            return resultTuples;
        }

        #region IReplica Implementation
        public void ProcessAndForward(CTuple tuple)
        {
            Console.WriteLine("Received " + tuple + " from " + tuple.opName);
            originOperators[tuple.opName][tuple.repID].Insert(tuple);
            Console.WriteLine("Successfully inserted");
        }

        public void Start()
        {
            if (!processingState)
            {
                OnStart.Invoke(this, new EventArgs());
                processingState = true;
            }

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
        public void Freeze()
        {
            Console.WriteLine("Freezing...");
            OnFreeze?.Invoke(this, new EventArgs());

        }
        public void Unfreeze()
        {
            Console.WriteLine($"Unfreezing...");
            OnUnfreeze?.Invoke(this, new EventArgs());
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
            StartFrom = state.LastEmittedTuple;
        }

        public void Resend(TupleID id, string operatorId, int replicaId)
        {
            if (operatorId == this.OperatorId && this.ID == replicaId)
                return;

            destinations[operatorId].Resend(id, replicaId);
        }

        public void GarbageCollect(TupleID id, string operatorId, int replicaId)
        {
            //Console.WriteLine("Garbage Collecting");
            destinations[operatorId].GarbageCollect(id, replicaId);
        }
    }


    public class IntervalEventArgs : EventArgs
    {
        public int Millis { get; set; }
    }


   
}
