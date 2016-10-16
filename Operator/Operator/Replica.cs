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
    class Replica : MarshalByRefObject, IReplica
    {
        public string OperatorId { get; }
        private readonly ProcessDelegate processFunction;
        private IList<NeighbourOperator> destinations;
        private IList<IReplica> otherReplicas;
        private IList<string> inputFiles;
        private bool shouldNotify = false;
        private bool isProcessing = false;
        //private Semantic semantic;
        public int totalSeenTuples = 0;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();
        

        public Replica(ReplicaCreationInfo rep)
        {
            var info = rep.Operator;
            this.OperatorId = info.ID;
            this.otherReplicas = info.Addresses.Select((address) => GetStub(address)).ToList();
            this.destinations = info.OutputOperators.Select((dstInfo) => new NeighbourOperator
            {
                replicas = dstInfo.Addresses.Select((address) => GetStub(address)).ToList()
            }).ToList();
            this.processFunction = Operations.GetOperation(info.OperatorFunction, info.OperatorFunctionArgs);
            this.shouldNotify = info.ShouldNotify;
            this.inputFiles = info.InputFiles;
        }

        // This method should get the remote object given its address
        private IReplica GetStub(string address)
        {
            throw new NotImplementedException();
        }
        private CTuple Process(CTuple tuple)
        {
            throw new NotImplementedException();
        }

        private void SendToAll(CTuple tuple)
        {
            throw new NotImplementedException();
        }

        #region IReplica Implementation
        public void ProcessAndForward(CTuple tuple)
        {
            var result = Process(tuple);
            SendToAll(result);
        }

        public void Start()
        {
            isProcessing = true;
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
