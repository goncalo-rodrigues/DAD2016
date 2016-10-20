using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    public delegate IEnumerable<IList<string>> ProcessDelegate(IList<string> tuple);
    // A delegate type for handling events from PuppetMaster
    public delegate void PuppetMasterEventHandler(object sender, EventArgs e);
    
    class Replica : MarshalByRefObject, IReplica
    {
        public string OperatorId { get; }
        private readonly ProcessDelegate processFunction;
        private IList<NeighbourOperator> destinations;
        private IList<IReplica> otherReplicas;
        private List<string> inputFiles;
        private bool shouldNotify = false;
        private bool isProcessing = false;
        private Semantic semantic;
        public int totalSeenTuples = 0;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();

        // event is raised when processing starts
        public event PuppetMasterEventHandler OnStart;

        public Replica(ReplicaCreationInfo rep)
        {
            var info = rep.Operator;
            this.OperatorId = info.ID;
            this.processFunction = Operations.GetOperation(info.OperatorFunction, info.OperatorFunctionArgs);
            this.shouldNotify = info.ShouldNotify;
            this.inputFiles = info.InputFiles;
            this.semantic = info.Semantic;


            this.OnStart += (sender, args) =>
            {
                this.otherReplicas = info.Addresses.Select((address) => Helper.GetStub<IReplica>(address)).ToList();
                this.destinations = info.OutputOperators.Select((dstInfo) => new NeighbourOperator(dstInfo)).ToList();
                isProcessing = true;
            };
            
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
                neighbor.send(tuple, semantic);
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
