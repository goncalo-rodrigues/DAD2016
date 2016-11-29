using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    [Serializable]
    public class ReplicaState
    {
        public object OperationInternalState { get; set; }
        public List<int> InputStreamsIds { get; set; }
        public int OutputId { get; set; }
    }
}
