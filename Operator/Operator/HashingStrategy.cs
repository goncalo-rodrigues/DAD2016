using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Operator
{
    /**
    * Hashing(field id): tuples are forwarded to a replica of the downstream operator according to the value of a hashing function computed 
    *                    on the tuple’s field with identifier field id.
    **/
    class HashingStrategy : RoutingStrategy
    {

        public HashingStrategy(List<Replica> replicas, int id, CTuple tuple):base(replicas, id, tuple){

        }

        public override Replica ChooseReplica()
        {

            //found replica with field_id = id
           /* foreach (CTuple tup in this.tuple) {
                if (rep.Des)
            }

            HashAlgorithm hash;
            hash.ComputeHash(this.list[]);
            */



            throw new NotImplementedException();
        }
    }
}
