using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace Operator
{
    public class MergedInBuffer : BufferedOperator
    {
        public List<OriginOperator> Origins;
        
        public MergedInBuffer(List<OriginOperator> buffersToMerge) : base(false) {
            Origins = buffersToMerge;
        }

        public override void DoStuff(CTuple tuple)
        {
            throw new NotImplementedException();
        }

        public CTuple Next()
        {
            if (Count() > 0)
            {
                return Take();
            }
            foreach (var origin in Origins)
            {
                Task.Run(() => Insert(origin.Take()));
            }
            return Take();
        }
    }
}
