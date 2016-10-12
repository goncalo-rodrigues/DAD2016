using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public class OperatorCreationInfo
    {
        public string ID { get; set; }
        public int ReplicationFactor { get; set; }
        public RoutingStrategy RtStrategy { get; set; }
        public List<string> InputOperators { get; set; }
        public List<DestinationInfo> OutputOperators { get; set; }
        public List<string> Addresses { get; set; } // of the multiple replicas
        public string OperatorFunction { get; set; }
        public List<string> OperatorFunctionArgs { get; set; }
        public int HashingArg { get; set; } // only applicable if routingstrat == hashing
        public Semantic Semantic { get; set; }
        public bool ShouldNotify { get; set; }
    }
}
