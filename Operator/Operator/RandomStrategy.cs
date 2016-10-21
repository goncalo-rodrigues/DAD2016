using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    /**
    * Random: tuples are output to a random replica of the downstream operator.
    **/
    class RandomStrategy : RoutingStrategy
    {
        public RandomStrategy(List<IReplica> replicas):base(replicas){

        }

        public override IReplica ChooseReplica()
        {
            Random rnd = new Random();
            int number = rnd.Next(0, this.list.Count);

            //TODO: verify if "crash flag" is on - after checkpoint
            return this.list[number];

        }
    }
}
