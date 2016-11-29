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
        public Dictionary<string, DestinationState> InputStreamsIds { get; set; }
        public Dictionary<string, DestinationState> OutputStreamsIds { get; set; }

    }
    [Serializable]
    public class DestinationState
    {
        public List<int> SentIds;
    }
}
