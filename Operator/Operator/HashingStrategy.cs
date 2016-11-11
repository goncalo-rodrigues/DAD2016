using SharedTypes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

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
        public HashingStrategy(List<IReplica> replicas, int id) : base(replicas)
        {
            this.id = id;

        }

        public override IReplica ChooseReplica(CTuple tuple) {

            String s = tuple.GetFieldByIndex(id);
          
            int i = s.GetHashCode();

            return (this.list[Math.Abs(i) %list.Count]);

        }

    }
}
