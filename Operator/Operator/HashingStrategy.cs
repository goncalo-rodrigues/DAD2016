using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    /**
    * Hashing(field id): tuples are forwarded to a replica of the downstream operator according to the 
    *                    value of a hashing function computed on the tuple’s field with identifier 
    *                    field id.
    **/
    class HashingStrategy : RoutingStrategy
    {

        public HashingStrategy(List<Replica> replicas, int id):base(replicas, id){

        }

        public override Replica chooseReplica()
        {
            throw new NotImplementedException();
        }
    }
}
