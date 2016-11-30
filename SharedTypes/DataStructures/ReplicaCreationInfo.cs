using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    [DataContract(Namespace = "tecnico.ulisboa.pt/2016/DADSTORM")]
    public class ReplicaCreationInfo
    {
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public OperatorInfo Operator { get; set; }
    }
}
