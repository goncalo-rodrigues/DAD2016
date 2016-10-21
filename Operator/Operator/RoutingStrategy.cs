using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    abstract class RoutingStrategy
    {
        private object p1;
        private object p2;

        public List<IReplica> list {
            get { return list; }
            set { }
        }

        /*public SharedTypes.CTuple tuple
        {
            get { return tuple; }
            set { }
        }
        public int field_id
        {
            get { return field_id; }
            set { }
        }*/

        public RoutingStrategy(List<IReplica> replicas)
        {
            list = replicas;
         
        }

       /* public RoutingStrategy(List<Replica> replicas, int id, SharedTypes.CTuple tup)
        {
            list = replicas;
            field_id = id;
            tuple = tup;
        }*/

     

        abstract public IReplica ChooseReplica();
    }
}


