﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class NeighbourOperator : Destination
    {
        public List<Replica> replicas;
        public override string send(CTuple tuple, int semantic)
        {
            throw new NotImplementedException();
        }
    }
}
