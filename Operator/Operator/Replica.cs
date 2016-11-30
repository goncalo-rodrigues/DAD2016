using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Windows;
using SharedTypes.PerfectFailureDetector;

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

        public bool IsPrimary { get; }
        public string OperatorId { get; }
        public int ID { get; set; }
        public string MasterURL { get; set; }
        public string selfURL { get; set; }
        public Dictionary<string, List<string>> InputOperators { get;  }
        public int TupleCounter { get; set; } = 0;

        public List<ReplicaState> OtherReplicasStates;

        private ILogger logger;
        
        private IDictionary<string, Destination> destinations;
        public IList<IReplica> otherReplicas;
        public IList<IReplica> inputReplicas;
        public List<string> adresses;
        private List<string> inputFiles;
        private RoutingStrategy routingStrategy;

        private Dictionary<string, List<OriginOperator>> originOperators;
        private OriginOperator inBuffer;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();
        private bool shouldNotify = false;
        private bool processingState = false;
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
            this.inBuffer = new OriginOperator();
            
            this.shouldNotify = info.ShouldNotify;
            this.inputFiles = info.InputFiles;
            // primary is the first one in the array
            this.IsPrimary = rep.Id == 0;
            this.selfURL = rep.Address;
            this.MasterURL = info.MasterURL;
            this.InputOperators = info.InputReplicas;
            this.ID = rep.Id;
          

            // ALSO MOVE DESTINATIONS TO ORIGIN'
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
          
            var initTask = Task.Run(async () =>
            {
                
                if (info.RtStrategy == SharedTypes.RoutingStrategy.Primary)
                {
                    this.routingStrategy = new PrimaryStrategy(adresses.Count);
                }
                else if (info.RtStrategy == SharedTypes.RoutingStrategy.Hashing)
                {
                    this.routingStrategy = new HashingStrategy(adresses.Count, info.HashingArg);
                }
                else
                {
                    this.routingStrategy = new RandomStrategy(adresses.Count, OperatorId.GetHashCode());
                }
                foreach (var op in this.InputOperators.Keys)
                {
                    this.inputReplicas = await Helper.GetAllStubs<IReplica>(this.InputOperators[op]);
                }

            });

            // Start reading from file(s)
            this.OnStart += (sender, args) =>
            {
                Task.Run(() =>
                {
                    foreach (var path in inputFiles)
                    {
                        StartProcessingFromFile(path);
                    }
                });

            };

            Task.Run(() => mainLoop());
            
            if (shouldNotify)
                //destinations.Add(new LoggerDestination(this, info.Semantic, $"{OperatorId}({ID})", MasterURL));
                destinations.Add("puppet_master_logger", new LoggerDestination(this, info.Semantic, selfURL, MasterURL));


                // Configure windows position
                Console.Title = this.OperatorId;
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
                
                var t = inBuffer.Take();
                var result = Process(t);
                foreach (var tuple in result)
                    SendToAll(tuple);
            }
        }

        public void StartProcessingFromFile(string path)
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
                        TupleCounter++;
                        var ctuple = new CTuple(tupleData, TupleCounter);
                        Console.WriteLine($"Read tuple from file: {ctuple}");
                        if (routingStrategy.ChooseReplica(ctuple) == ID)
                        {
                            ProcessAndForward(ctuple);
                        }
                    }
                }
            }
            catch (IOException e)
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
            var resultData = ProcessFunction.Process(data);
            resultTuples = resultData.Select((tupleData) => new CTuple(tupleData.ToList(), tuple.ID));
            Console.WriteLine($"Processed {tuple.ToString()}");
            return resultTuples;
        }

        #region IReplica Implementation
        public void ProcessAndForward(CTuple tuple)
        {
            inBuffer.Insert(tuple);
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
        public void Status()
        {
            // print state of the system
            string status = "[Operator: " + OperatorId + ", Status: " + (processingState == true ? "Processing" : "Not Processing");
           
            int neighboursCnt = 0;
            int repCnt = 0;

            IList<Destination> onlyOperatorDestinations = null;
            if (!shouldNotify || destinations == null)
            {

                onlyOperatorDestinations = destinations.Values.ToList();
            }
            else
            {

                // remove logger from destinations list (https://msdn.microsoft.com/en-us/library/bb549418.aspx)
                onlyOperatorDestinations = destinations.Values.Where((dest, index) => index < destinations?.Count - 1).ToList();
            }

            if (onlyOperatorDestinations != null && onlyOperatorDestinations.Count > 0)
            {
                foreach (Destination neighbour in onlyOperatorDestinations)
                {
                    try
                    {
                        if (neighbour != null )
                        {
                            var task = Task.Run(() => neighbour.Ping());
                            if (task.Wait(TimeSpan.FromMilliseconds(100)))
                            {
                                neighboursCnt++;
                            }
                        }
                    }
                    catch (AggregateException e)
                    {
                        // does nothing
                    }
                    
                }
                status += $", Neighbours: {(neighboursCnt)} (of {(onlyOperatorDestinations.Count)})";
            }
            else
            {
                status += $", Neighbours: 0 (of 0)";
            }
              
            if (otherReplicas != null && otherReplicas.Count >= 0)
            {
                foreach (IReplica irep in otherReplicas)
                {
                    try
                    {
                        if (irep != null)
                        {
                            var task = Task.Run(() => irep.Ping());
                            if (task.Wait(TimeSpan.FromMilliseconds(100)))
                                repCnt++;
                        }
                    }
                    catch (Exception e)
                    {
                        // does nothing
                    }
                }
                status += $", Working Replicas: {(repCnt+1)} (of {(otherReplicas?.Count)})";
            } 
            else
            {
                //myself only
                status += $", Working Replicas: 1 (of 1)";
            }
            Console.WriteLine(status);
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

        public void Finish()
        {
           //  if (/*todo: if all input operators have called this method */false)
                inBuffer.MarkFinish();
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


        /*Just to configure windows position - END*/


        public ReplicaState GetState()
        {
            var result = new ReplicaState();
            // result.OperationInternalState = processFunction.InternalState;
            result.OutputStreamsIds = new Dictionary<string, DestinationState>();
            foreach (var d in destinations)
            {
                var state = d.Value.GetState();
                if (state != null)
                    result.OutputStreamsIds[d.Key] = state;
            }
            return result;
        }

        public void LoadState(ReplicaState state)
        {
            // processFunction.InternalState = state.OperationInternalState;
            foreach (var d in state.OutputStreamsIds)
            {
                destinations[d.Key].LoadState(d.Value);
            }
            
            // ask to resend
        }

        public void Resend(int id, string operatorId, int replicaId)
        {
            destinations[operatorId].Resend(id, replicaId);
        }
        public void GarbageCollect(int id, string operatorId, int replicaId)
        {
            destinations[operatorId].GarbageCollect(id, replicaId);
        }
    }


    public class IntervalEventArgs : EventArgs
    {
        public int Millis { get; set; }
    }


   
}
