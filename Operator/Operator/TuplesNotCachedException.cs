using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    public class TuplesNotCachedException : Exception
    {
        public int FirstTupleNotCached { get; set; }
        public int LastTupleNotCached { get; set; }
        public TuplesNotCachedException(int idFirstTuple, int idLastTuple)
        {
            FirstTupleNotCached = idFirstTuple;
            LastTupleNotCached = idLastTuple;
        }
    }
}
