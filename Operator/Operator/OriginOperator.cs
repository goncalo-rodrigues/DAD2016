using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace Operator
{
    public class OriginOperator : BufferedOperator
    {
        public OriginOperator() : base(false) {
        }

        public override CTuple Take()
        {
            var tup = base.Take();
            // update stuff
            return tup;
        }
        public override void DoStuff(CTuple tuple)
        {
            // Not needed
        }
    
    }
}
