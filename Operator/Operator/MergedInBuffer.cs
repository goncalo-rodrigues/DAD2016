using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace Operator
{
    public class MergedInBuffer
    {
        public List<OriginOperator> Origins;
        public MergedInBuffer(List<OriginOperator> buffersToMerge) {
            Origins = buffersToMerge;
        }

        public CTuple Next()
        {
            // do merge magic here

            return null;
        }
    }
}
