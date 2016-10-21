﻿using SharedTypes;
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
    public delegate IEnumerable<IList<string>> ProcessDelegate(IList<string> tuple);
    // A delegate type for handling events from PuppetMaster
    public delegate void PuppetMasterEventHandler(object sender, EventArgs e);
    
    class Replica : MarshalByRefObject, IReplica
    {

        public string MasterURL { get; set; }
        public string selfURL { get; set; }
        public string OperatorId { get; }
        public int totalSeenTuples = 0;
        public ConcurrentDictionary<string, bool> SeenTupleFieldValues = new ConcurrentDictionary<string, bool>();

        private ILogger logger;
        private readonly ProcessDelegate processFunction;
        private IList<NeighbourOperator> destinations;
        private IList<IReplica> otherReplicas;
        private List<string> inputFiles;
        private bool shouldNotify = false;
        private bool isProcessing = false;
        private Semantic semantic;

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

            this.OnStart += (sender, args) =>
            {
                this.otherReplicas = info.Addresses.Select((address) => Helper.GetStub<IReplica>(address)).ToList();
                this.destinations = info.OutputOperators.Select((dstInfo) => new NeighbourOperator(dstInfo)).ToList();
                isProcessing = true;
            };

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
            using (var f = new StreamReader(path))
            {
                string line = null;
                while ((line = f.ReadLine()) !=  null)
                {
                    var tupleData = line.Split(null).ToList();
                    ProcessAndForward(new CTuple(tupleData));
                }
            }
        }

        public void InitPMLogService()
        {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);
            
            logger = (ILogger) Activator.GetObject(typeof(ILogger), MasterURL);

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
                if (shouldNotify && logger != null)
                    Notify(tuple);
                neighbor.send(tuple, semantic);
            }
        }

        private void Notify(CTuple tuple) {
            try
            {
                String content = $"tuple {selfURL}, {tuple.ToString()}";
                logger.Notify(new Record(content, DateTime.Now));
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
