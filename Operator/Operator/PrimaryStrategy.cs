using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{

    /**
    * Primary: tuples are output to the primary replica of the operator it is connected to downstream.
    **/
    class PrimaryStrategy : RoutingStrategy
    {
        public PrimaryStrategy(List<Replica> replicas):base(replicas){
           
        }


        public override Replica chooseReplica()
        {
            //TODO: verify if "crash flag" is on - after checkpoint
            return this.list[0];
        }
    }
}
