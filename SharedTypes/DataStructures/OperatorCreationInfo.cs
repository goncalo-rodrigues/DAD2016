using System.Collections.Generic;

namespace SharedTypes
{
    public class OperatorInfo
    {
        public string ID { get; set; }
        public string MasterURL { get; set; }
        public int ReplicationFactor { get; set; }
        public RoutingStrategy RtStrategy { get; set; }
        public List<string> InputOperators { get; set; }
        public List<string> InputFiles { get; set; }
        public Dictionary<string, List<string>> InputReplicas { get; set; }
        public List<DestinationInfo> OutputOperators { get; set; }
        public List<string> Addresses { get; set; } // of the multiple replicas
        public string OperatorFunction { get; set; }
        public List<string> OperatorFunctionArgs { get; set; }
        public int HashingArg { get; set; } // only applicable if routingstrat == hashing
        public Semantic Semantic { get; set; }
        public bool ShouldNotify { get; set; }
    }
}
