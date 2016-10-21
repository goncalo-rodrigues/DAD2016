﻿using System;
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

        public List<Replica> list {
            get { return list; }
            set { }
        }

        public CTuple tuple
        {
            get { return tuple; }
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



        public RoutingStrategy(List<Replica> replicas, int id, CTuple tup)
        {
            list = replicas;
            field_id = id;
            tuple = tup;
        }

     

        abstract public Replica ChooseReplica();
    }
}


