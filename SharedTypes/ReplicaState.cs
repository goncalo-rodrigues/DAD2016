using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    [Serializable]
    public class ReplicaState
    {
        public object OperationInternalState { get; set; }
        public Dictionary<string, OriginState> InputStreamsIds { get; set; } 
        public Dictionary<string, DestinationState> OutputStreamsIds { get; set; }
        public int LastEmittedTuple { get; set; } // in case it reads from files

    }
    [Serializable]
    public class DestinationState
    {
        public List<int> SentIds;
    }

    [Serializable]
    public class OriginState
    {
        public List<int> SentIds;
    }
}
