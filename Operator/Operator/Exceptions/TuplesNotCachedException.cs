using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    public class TuplesNotCachedException : Exception
    {
        public TupleID FirstTupleNotCached { get; set; }
        public TupleID LastTupleNotCached { get; set; }
        public TuplesNotCachedException(TupleID idFirstTuple, TupleID idLastTuple) : base($"Tuples from {idFirstTuple} up to {idLastTuple} were not found in cache")
        {
            FirstTupleNotCached = idFirstTuple;
            LastTupleNotCached = idLastTuple;
        }
    }
}
