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


namespace Operator
{
    // Delegate for calling other remote operators asynchronously
    public delegate void RemoteProcessAsyncDelegate(CTuple tuple);
    public delegate IEnumerable<IList<string>> ProcessDelegate(IList<string> tuple);
    // A delegate type for handling events from PuppetMaster
    public delegate void PuppetMasterEventHandler(object sender, EventArgs e);
    public delegate void PuppetMasterIntervalEventHandler(object sender, IntervalEventArgs e);


    public class Replica : MarshalByRefObject, IReplica
    {

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

        private ILogger logger;

        private readonly ProcessDelegate processFunction;
        private IList<Destination> destinations;
        public IList<IReplica> otherReplicas;
        private List<string> inputFiles;


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
            this.ID = info.Addresses.IndexOf(rep.Address);

            if (info.OutputOperators == null || info.OutputOperators.Count == 0)
            {
                // it is an output operator
                this.destinations = new List<Destination> { new OutputFile(this, info.Semantic) };
            } else
            {
                this.destinations = info.OutputOperators.Select((dstInfo) => (Destination)new NeighbourOperator(this, dstInfo, info.Semantic)).ToList();
            }
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
            });


            // Start reading from file(s)
            this.OnStart += (sender, args) =>
            {
                foreach (var path in inputFiles)
                {
                    Task.Run(() => StartProcessingFromFile(path));
                }
            };
            if (shouldNotify)
                destinations.Add(new LoggerDestination(this, info.Semantic, $"{OperatorId}({ID})", MasterURL));

            // Configure windows position
            Console.Title = this.OperatorId;
            string a = this.OperatorId;

            if (a.Equals("OP1")) { SetWindowPosition(600, 0, 400, 200); }
            if (a.Equals("OP3")) { SetWindowPosition(950, 0, 400, 200); }
            if (a.Equals("OP4")) { SetWindowPosition(600, 200, 400, 200); }
            if (a.Equals("OP5")) { SetWindowPosition(950, 200, 400, 200); }
            if (a.Equals("OP6")) { SetWindowPosition(600, 400, 400, 200); }
            if (a.Equals("OP7")) { SetWindowPosition(950, 400, 400, 200); }
            if (a.Equals("OP8")) { SetWindowPosition(600, 600, 400, 200); }
            if (a.Equals("OP9")) { SetWindowPosition(950, 600, 400, 200); }
            if (a.Equals("OP10")) { SetWindowPosition(600, 800, 400, 200); }

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
                        var ctuple = new CTuple(tupleData);
                        Console.WriteLine($"Read tuple from file: {ctuple}");
                        if (routingStrategy.ChooseReplica(ctuple) == null)
                        {
                            ThreadPool.QueueUserWorkItem((x) => this.ProcessAndForward((CTuple)x), ctuple);
                        }   
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine($"Unable to read from file {path}. Exception: {e.Message}.");
            }
        }

 
        private IEnumerable<CTuple> Process(CTuple tuple)
        {
            IEnumerable<CTuple> resultTuples = null;
            // debug print 
            Console.WriteLine($"Received {tuple.ToString()}");

            var data = tuple.GetFields();
            var resultData = processFunction(data);
            resultTuples = resultData.Select((tupleData) => new CTuple(tupleData.ToList()));
            
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
            var result = Process(tuple);
            //Console.WriteLine($"Operator {OperatorId} has received the following tuple: {tuple.ToString()}");
            foreach (var tup in result)
            {
                SendToAll(tup);
            }
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
            OnFreeze?.Invoke(this, new EventArgs());

        }
        public void Unfreeze()
        {
            Console.WriteLine($"Unfreezing...");
            OnUnfreeze?.Invoke(this, new EventArgs());
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
    }

    public class IntervalEventArgs : EventArgs
    {
        public int Millis { get; set; }
    }


   
}
