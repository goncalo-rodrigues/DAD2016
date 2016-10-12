using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public class DestinationInfo
    {
        public string ID { get; set; }
        public int ReplicationFactor { get; set; }
        public RoutingStrategy RtStrategy { get; set; }
        public List<string> Addresses { get; set; }
    }
}
