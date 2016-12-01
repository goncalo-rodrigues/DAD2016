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
        public TuplesNotCachedException(TupleID idFirstTuple, TupleID idLastTuple)
        {
            FirstTupleNotCached = idFirstTuple;
            LastTupleNotCached = idLastTuple;
        }
    }
}
