using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    public abstract class RoutingStrategy
    {
        private object p1;
        private object p2;

        public List<IReplica> list { get; set; }

        public RoutingStrategy(List<IReplica> replicas)
        {
            list = replicas;
         
        }

        abstract public int ChooseReplica(CTuple tuple);

     
    }
}


