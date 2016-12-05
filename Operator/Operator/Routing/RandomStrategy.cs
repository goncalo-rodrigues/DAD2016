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
        public Random random { get; set; }
        public RandomStrategy(int countRep, int? seed = null):base(countRep)
        {
            random = seed == null ? new Random() : new Random(seed ?? 0);
            this.countRep = countRep;

        }

        public override int ChooseReplica(CTuple tuple)
        {
            int number = random.Next(0, countRep);

            //TODO: verify if "crash flag" is on - after checkpoint
            return number;

        }

        public override object GetState()
        {
            return random;
        }

        public override void LoadState(object state)
        {
            this.random = (Random)state;
        }
    }
}
