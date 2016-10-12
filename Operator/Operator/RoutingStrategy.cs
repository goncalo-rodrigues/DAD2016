using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    abstract class RoutingStrategy
    {
        public List<Replica> list {
            get { return list; }
            set { }
        }
        public int field_id
        {
            get { return field_id; }
            set { }
        }

       
        public RoutingStrategy(List<Replica> replicas)
        {
            list = replicas;
        }

       public RoutingStrategy(List<Replica> replicas, int id)
        {
            list = replicas;
            field_id = id;
        }

       abstract public Replica chooseReplica();
    }
}


