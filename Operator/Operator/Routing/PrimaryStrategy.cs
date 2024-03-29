﻿using SharedTypes;
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
        public PrimaryStrategy(int countRep) :base(countRep){
            this.countRep = countRep;
        }


        public override int ChooseReplica(CTuple tuple)
        {
            //TODO: verify if "crash flag" is on - after checkpoint
            //      verify that it exists at least one element in the list
            return 0;
        }
    }
}
