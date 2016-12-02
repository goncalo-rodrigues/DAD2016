using SharedTypes;
using System;

namespace Operator
{
    /**
    * Hashing(field id): tuples are forwarded to a replica of the downstream operator according to the 
    *                    value of a hashing function computed on the tuple’s field with identifier 
    *                    field id.
    **/
    class HashingStrategy : RoutingStrategy
    {
        private int id { get; set; }
        public HashingStrategy(int countRep, int id) : base(countRep)
        {
            this.id = id;
            this.countRep = countRep;

        }

        public override int ChooseReplica(CTuple tuple) {

            String s = tuple.GetFieldByIndex(id);
          
            int i = s.GetHashCode();

            return (Math.Abs(i) % countRep);

        }

    }
}
