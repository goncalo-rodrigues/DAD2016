using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace Operator
{
    public class MergedInBuffer
    {
        public List<OriginOperator> Origins;
        private List<CTuple> currentTuple;
        
        public MergedInBuffer(List<OriginOperator> buffersToMerge) {
            Origins = buffersToMerge;
            currentTuple = new List<CTuple>(new CTuple[Origins.Count]);
        }

        public CTuple Next()
        {
            int chosen = -1;
            CTuple chosenTuple = null;

            // takes a tuple from each origin and puts it into currentTuple list
            for (int i = 0; i < Origins.Count; i++)
            {
               // Console.WriteLine($"Waiting for {Origins[i].OpId} ({Origins[i].ReplicaId})");
                if (currentTuple[i] != null) continue;
                currentTuple[i] = Origins[i].Take();
            }
            // chooses one
            chosen = Choose(currentTuple);
            // non-data tuple (could be from flush)
            chosenTuple = currentTuple[chosen];
            currentTuple[chosen] = null;
            //Console.WriteLine($"Chosen = {chosen}");
            
            
            return chosenTuple;
        }

        private int Choose(IList<CTuple> tuples)
        {
            CTuple currentMin = tuples[0];
            int currentMinIndex = 0;
            for(int i=1; i < tuples.Count; i++)
            {
                var newTuple = tuples[i];
                
                if (newTuple.ID < currentMin.ID || (newTuple.ID == currentMin.ID && i > currentMinIndex))
                {
                    currentMinIndex = i;
                    currentMin = newTuple;
                }
            }
            return currentMinIndex;
        }
    }
}
