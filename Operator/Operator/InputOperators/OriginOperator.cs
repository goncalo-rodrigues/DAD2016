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
        public string OpId { get; }
        public int ReplicaId { get; }
        public TupleID LastProcessedId { get; set; } = new TupleID();
        public OriginOperator(string opId, int replicaId) : base(false) {
            this.OpId = opId;
            this.ReplicaId = replicaId;
        }

        public override CTuple Take()
        {
           
            var tup = base.Take();
            // update stuff
            return tup;
        }
        public override void DoStuff(CTuple tuple)
        {
            throw new NotImplementedException();
            // Not needed
        }
    
    }
}
