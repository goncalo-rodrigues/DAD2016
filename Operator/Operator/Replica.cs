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
        public string FunctionString { get; }
        public List<string> InputOperators { get;  }
        public int TupleCounter { get; set; } = 0;

        private ILogger logger;

        private Operation processFunction;
        private IList<Destination> destinations;
        public IList<IReplica> otherReplicas;
        public IList<IReplica> inputReplicas;
        public List<string> adresses;
        private List<string> inputFiles;
        private PerfectFailureDetector perfectFailureDetector;
        private BlockingCollection<CTuple> tuplesToProcess;



        private RoutingStrategy routingStrategy;

        public int totalSeenTuples = 0;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();
        private bool shouldNotify = false;
        private bool processingState = false;

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
            this.processFunction = Operations.GetOperation(info.OperatorFunction, info.OperatorFunctionArgs);
            this.FunctionString = info.OperatorFunction;
            this.shouldNotify = info.ShouldNotify;
            this.inputFiles = info.InputFiles;
            // primary is the first one in the array
            this.IsPrimary = rep.Address == info.Addresses[0];
            this.selfURL = rep.Address;
            this.MasterURL = info.MasterURL;
            this.InputOperators = info.InputReplicas;
            this.ID = info.Addresses.IndexOf(selfURL);
            Console.WriteLine("***url.: " + selfURL);
            Console.WriteLine("***ID.: " +ID);
            if (info.OutputOperators == null || info.OutputOperators.Count == 0)
            {
                // it is an output operator
                this.destinations = new List<Destination> { new OutputFile(this, info.Semantic) };
            } else
            {
                this.destinations = info.OutputOperators.Select((dstInfo) => (Destination)new NeighbourOperator(this, dstInfo, info.Semantic)).ToList();
            }
            this.perfectFailureDetector = new PerfectFailureDetector();
            this.perfectFailureDetector.NodeFailed += (sender, args) => {
                Console.WriteLine($"{args.FailedNodeName} failed");
            };
            this.perfectFailureDetector.NodeFailed += OnFail;
            var initTask = Task.Run(async () =>
            {
                this.otherReplicas =
                (await Helper.GetAllStubs<IReplica>(
                    // hack to not get his own stub
                    info.Addresses.Select((address) => (selfURL != address ? address : null)).ToList()))
                    .ToList();
                var allReplicas = (new List<IReplica>(otherReplicas));


                if (info.RtStrategy == SharedTypes.RoutingStrategy.Primary)
                {
                    this.routingStrategy = new PrimaryStrategy(allReplicas);
                }
                else if (info.RtStrategy == SharedTypes.RoutingStrategy.Hashing)
                {
                    this.routingStrategy = new HashingStrategy(allReplicas, info.HashingArg);
                }
                else
                {
                    this.routingStrategy = new RandomStrategy(allReplicas, OperatorId.GetHashCode());
                }

                this.inputReplicas = await Helper.GetAllStubs<IReplica>(this.InputOperators);
                for (int i = 0; i < info.Addresses.Count; i++)
                {
                    this.adresses.Add(info.Addresses[i]);
                    if (info.Addresses[i] != selfURL)
                        perfectFailureDetector.StartMonitoringNewNode(info.Addresses[i], allReplicas[i]);
                }
            });




            // Start reading from file(s)
            this.OnStart += (sender, args) =>
            {
                foreach (var path in inputFiles)
                {
                    StartProcessingFromFile(path);
                }
            };

            tuplesToProcess = new BlockingCollection<CTuple>(new ConcurrentQueue<CTuple>(), BUFFER_SIZE);

            Task.Run(() => Processor());

            
            if (shouldNotify)
                //destinations.Add(new LoggerDestination(this, info.Semantic, $"{OperatorId}({ID})", MasterURL));
                destinations.Add(new LoggerDestination(this, info.Semantic, selfURL, MasterURL));


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
                        var ctuple = new CTuple(tupleData, TupleCounter, TupleCounter);
                        Console.WriteLine($"Read tuple from file: {ctuple}");
                        if (routingStrategy.ChooseReplica(ctuple) == ID)
                        {
                            ProcessAndForward(ctuple);
                        }   
                    }
                }
            } catch (IOException e)
            {
                Console.WriteLine($"Unable to read from file {path}. Exception: {e.Message}.");
            }
        }

 
        private IEnumerable<CTuple> Process(CTuple tuple)
        {
            IEnumerable<CTuple> resultTuples = null;
            // debug print 
            

            var data = tuple.GetFields();
            var resultData = processFunction.Process(data);
            resultTuples = resultData.Select((tupleData) => new CTuple(tupleData.ToList(), tuple.ID, TupleCounter++));
            Console.WriteLine($"Processed {tuple.ToString()}");
            return resultTuples;
        }

        private void SendToAll(CTuple tuple)
        {
             foreach (var neighbor in destinations)
            {
                Task.Run(()=>neighbor.Send(tuple));
            }
        }

        #region IReplica Implementation
        public void ProcessAndForward(CTuple tuple)
        {
            Console.WriteLine($"Process And Forward {tuple.ToString()}");
            tuplesToProcess.Add(tuple);
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

                onlyOperatorDestinations = destinations;
            }
            else
            {

                // remove logger from destinations list (https://msdn.microsoft.com/en-us/library/bb549418.aspx)
                onlyOperatorDestinations = destinations.Where((dest, index) => index < destinations?.Count - 1).ToList();
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
            if (/*todo: if all input operators have called this method */false)
                tuplesToProcess.CompleteAdding();
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

        private void Processor()
        {
            while (!tuplesToProcess.IsCompleted)
            {
                CTuple tuple = null;
                // Blocks if number.Count == 0
                // IOE means that Take() was called on a completed collection.
                // Some other thread can call CompleteAdding after we pass the
                // IsCompleted check but before we call Take. 
                // In this example, we can simply catch the exception since the 
                // loop will break on the next iteration.
                try
                {
                    tuple = tuplesToProcess.Take();
                }
                catch (InvalidOperationException) { }

                if (tuple != null)
                {

                    var result = Process(tuple);
                    Console.WriteLine($"Operator {OperatorId} has received the following tuple: {tuple.ToString()}");
                    foreach (var tup in result)
                    {
                        SendToAll(tup);
                    }
                    //tuple é processado -> inserir na lista aqui o seu id (fazer depois da replicaçao)

                }
            }
        }


        public ReplicaState GetState()
        {
            var result = new ReplicaState();
            result.OperationInternalState = processFunction.InternalState;
            return result;
        }
    }

    public class IntervalEventArgs : EventArgs
    {
        public int Millis { get; set; }
    }


   
}
