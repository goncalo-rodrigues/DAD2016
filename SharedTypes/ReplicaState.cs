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

        public override string ToString()
        {
            string result = "<";
            foreach(var opName in InputStreamsIds.Keys)
            {
                result += opName + ": <";
                var state = InputStreamsIds[opName];
                result += string.Join(",", state.SentIds);

                result += ">";
            }
            result += ">";
            return result;
        }

    }
    [Serializable]
    public class DestinationState
    {
        public List<TupleID> SentIds;
        public List<CTuple> CachedOutputTuples;
    }

    [Serializable]
    public class OriginState
    {
        public List<TupleID> SentIds;
    }
}
