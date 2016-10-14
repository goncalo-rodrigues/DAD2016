﻿using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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

        public void ProcessAndForward(CTuple tuple)
        {
            var result = Process(tuple);
            SendToAll(result);
        }
    }
}
