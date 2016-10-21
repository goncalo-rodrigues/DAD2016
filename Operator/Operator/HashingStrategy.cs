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

        public HashingStrategy(List<IReplica> replicas):base(replicas){

        }

        public override IReplica ChooseReplica()
        {
            throw new NotImplementedException();
        }

        public IReplica ChooseReplica(int id, CTuple tuple) {

            String s = tuple.GetFieldByIndex(id);

            int i = s.GetHashCode();

            return (this.list[i%list.Count]);


        }

    }
}
