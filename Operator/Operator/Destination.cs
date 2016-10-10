using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    abstract class Destination
    {

        abstract public String send(CTuple tuple, int semantic);

    }
}
