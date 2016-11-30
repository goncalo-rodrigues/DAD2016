using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SharedTypes
{
    [DataContract(Namespace = "tecnico.ulisboa.pt/2016/DADSTORM")]
    public class OperatorInfo
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string MasterURL { get; set; }
        [DataMember]
        public int ReplicationFactor { get; set; }
        [DataMember]
        public RoutingStrategy RtStrategy { get; set; }
        [DataMember]
        public List<string> InputOperators { get; set; }
        [DataMember]
        public List<string> InputFiles { get; set; }
        [DataMember]
        public Dictionary<string, List<string>> InputReplicas { get; set; }
        [DataMember]
        public List<DestinationInfo> OutputOperators { get; set; }
        [DataMember]
        public List<string> Addresses { get; set; } // of the multiple replicas
        [DataMember]
        public string OperatorFunction { get; set; }
        [DataMember]
        public List<string> OperatorFunctionArgs { get; set; }
        [DataMember]
        public int HashingArg { get; set; } // only applicable if routingstrat == hashing
        [DataMember]
        public Semantic Semantic { get; set; }
        [DataMember]
        public bool ShouldNotify { get; set; }
    }
}
