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

namespace Operator
{
    // Delegate for calling other remote operators asynchronously
    public delegate void RemoteProcessAsyncDelegate(CTuple tuple);
    public delegate IEnumerable<IList<string>> ProcessDelegate(IList<string> tuple);
    // A delegate type for handling events from PuppetMaster
    public delegate void PuppetMasterEventHandler(object sender, EventArgs e);
    
    class Replica : MarshalByRefObject, IReplica
    {
        public string OperatorId { get; }
        public string MasterURL { get; set; }
        public string selfURL { get; set; }

        private ILogger logger;

        private readonly ProcessDelegate processFunction;
        private IList<NeighbourOperator> destinations;
        private IList<IReplica> otherReplicas;
        private List<string> inputFiles;

        private Semantic semantic;
        private RoutingStrategy routingStrategy;

        public int totalSeenTuples = 0;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();
        private bool shouldNotify = false;
        private bool isProcessing = false;

        // event is raised when processing starts
        public event PuppetMasterEventHandler OnStart;

        public Replica(ReplicaCreationInfo rep)
        {
            var info = rep.Operator;
            this.OperatorId = info.ID;
            this.MasterURL = info.MasterURL;
            this.processFunction = Operations.GetOperation(info.OperatorFunction, info.OperatorFunctionArgs);
            this.shouldNotify = info.ShouldNotify;
            this.inputFiles = info.InputFiles;
            this.semantic = info.Semantic;


            this.selfURL = rep.Address;
            this.MasterURL = info.MasterURL;

            // Get Stubs
            this.OnStart += (sender, args) =>
            {
                Console.WriteLine("Starting...");
                this.otherReplicas = info.Addresses.Select((address) => Helper.GetStub<IReplica>(address)).ToList();
                this.destinations = info.OutputOperators.Select((dstInfo) => new NeighbourOperator(dstInfo)).ToList();

                var allReplicas = (new List<IReplica>(otherReplicas));
                allReplicas.Add(this);
                // check if it sorts the same way for every remote replica
                allReplicas.Sort();
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
                    this.routingStrategy = new RandomStrategy(allReplicas);
                }
                isProcessing = true;
            };

            // Start reading from file(s)
            this.OnStart += (sender, args) =>
            {
                foreach (var path in inputFiles)
                {
                    Task.Run(() => StartProcessingFromFile(path));
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
                        if (routingStrategy.ChooseReplica() == this)
                        {
                            var tupleData = line.Split(',').Select((x) => x.Trim()).ToList();
                            var ctuple = new CTuple(tupleData);
                            new Thread(() =>ProcessAndForward(ctuple)).Start();
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
            //TcpChannel channel = new TcpChannel();
            //ChannelServices.RegisterChannel(channel, false);

            //logger = (ILogger) Activator.GetObject(typeof(ILogger), MasterURL);
            Console.WriteLine($"MasterURL: {MasterURL}");
            if (logger == null) //debug purposes
            {
                System.Console.WriteLine("Could not locate server");
            }
        }
 
        private CTuple Process(CTuple tuple)
        {
            var data = tuple.GetFields();
            var resultData = processFunction(data);
            var resultTuple = new CTuple(data.ToList());
            return resultTuple;
        }

        private void SendToAll(CTuple tuple)
        {
            foreach (var neighbor in destinations)
            {
               // if (shouldNotify && logger != null)
                 //   Notify(tuple);
                neighbor.Send(tuple, semantic);
            }
        }

        private void Notify(CTuple tuple) {
            try
            {
                String content = $"tuple {selfURL}, {tuple.ToString()}";
              //  if(logger!=null)
                //    logger.Notify(new Record(content, DateTime.Now));
            }
            catch (SocketException) // Neste caso queremos voltar a tentar ligaçao? -- modelo de faltas...
            {
                System.Console.WriteLine("Could not locate server");
            }
        }

        #region IReplica Implementation
        public void ProcessAndForward(CTuple tuple)
        {
            
            var result = Process(tuple);
            SendToAll(result);
        }

        public void Start()
        {
            OnStart.Invoke(this, new EventArgs());
        }

        public void Interval(int mils)
        {
            Thread.Sleep(mils);
        }

        public void Status()
        {
            // print state of the system
            string status = "[Operator: " + OperatorId + ", Status: " + (isProcessing == true ? "Working ," : "Not Working ,");
            int neighboursCnt = 0;
            int repCnt = 0;
            foreach (NeighbourOperator neighbour in destinations)
            {
                try
                {
                    var task = Task.Run(() => neighbour.Ping());
                    if (task.Wait(TimeSpan.FromMilliseconds(100)))
                        neighboursCnt++;
                } catch (Exception e)
                {
                    // does nothing
                }

                status += "Neighbours: " + neighboursCnt + "(of " + destinations.Count +"), ";

                foreach (IReplica irep in otherReplicas)
                {
                    try
                    {
                        var task = Task.Run(() => irep.Ping());
                        if (task.Wait(TimeSpan.FromMilliseconds(100)))
                            repCnt++;
                    }
                    catch (Exception e)
                    {
                        // does nothing
                    }
                }

                status += "Working Replicas: " + repCnt + " (of " + otherReplicas.Count +")]";
                Console.WriteLine(status);
            }
        }

        public void Ping()
        {
            Console.WriteLine($"{OperatorId} was pinged...");
        }

#endregion
    }
}
