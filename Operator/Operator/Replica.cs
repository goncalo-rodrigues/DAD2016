using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

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
        public bool IsPrimary { get; }
        public string OperatorId { get; }
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
                Console.WriteLine("initialize routing");
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
                    if (!processingState)
                    {
                        Console.WriteLine("Starting...");
                        foreach (var path in inputFiles)
                        {
                            Task.Run(() => StartProcessingFromFile(path));
                        }
                        Console.WriteLine("Started");
                    }
                };
            if (shouldNotify)
                InitPMLogService();
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

        public void InitPMLogService()
        {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);

            logger = (ILogger) Activator.GetObject(typeof(ILogger), MasterURL);
            if (logger != null)
                Console.WriteLine($"Could not locate server >>>> {MasterURL}");
            else
                Console.WriteLine($"PMLogService was successfully initiated: {logger}");
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
                 if (shouldNotify)
                       Notify(tuple);
                Task.Run(()=>neighbor.Send(tuple));
            }
        }

        private void Notify(CTuple tuple) {
            try
            {
                String content = $"tuple {selfURL}, {tuple.ToString()}";
                if(logger!=null)
                    logger.Notify(new Record(content, DateTime.Now));
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not locate server");
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
            if (destinations != null && destinations.Count > 0)
            {
                foreach (Destination neighbour in destinations)
                {
                    try
                    {
                        if (neighbour != null)
                        {
                            var task = Task.Run(() => neighbour.Ping());
                            if (task.Wait(TimeSpan.FromMilliseconds(100)))
                                neighboursCnt++;
                        }
                    }
                    catch (AggregateException e)
                    {
                        // does nothing
                    }
                    status += $", Neighbours: {(neighboursCnt)} (of {(destinations.Count)})";
                }
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
                status += $", Working Replicas: {(repCnt)} (of {(otherReplicas?.Count)})";
            } 
            else
            {
                status += $", Working Replicas: 0 (of 0)";
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
            Console.WriteLine("Invoking unfreeze");
            Console.WriteLine(OnUnfreeze);
            OnUnfreeze?.Invoke(this, new EventArgs());
        }
        #region COOPERATION
        public int IncrementCount()
        {
            return Interlocked.Increment(ref totalSeenTuples);
        }

        public bool TryAddSeenField(string fieldval)
        {
            return SeenTupleFieldValues.TryAdd(fieldval, true);
        }
        #endregion COOPERATION

        #endregion
    }

    public class IntervalEventArgs : EventArgs
    {
        public int Millis { get; set; }
    }
}
