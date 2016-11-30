using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace Operator
{
    
    class OriginOperator : BufferedOperator
    {
        private string functionName;
        private Operation processFunction;

        private IDictionary<string, Destination> destinations;

        public OriginOperator(string functionName, Operation processFunction) : base() {
            this.functionName = functionName;
            this.processFunction = processFunction;
        }

        public override void DoStuff(CTuple tuple)
        {
            var result = Process(tuple);
            foreach (var tup in result)
           {
                // SendToAll(tup);
           }
                //tuple é processado -> inserir na lista aqui o seu id (fazer depois da replicaçao)
      
        }
        
        private IEnumerable<CTuple> Process(CTuple tuple)
        {
            IEnumerable<CTuple> resultTuples = null;
            // debug print 


            var data = tuple.GetFields();
            var resultData = processFunction.Process(data);
            resultTuples = resultData.Select((tupleData) => new CTuple(tupleData.ToList(), tuple.ID));
            Console.WriteLine($"Processed {tuple.ToString()}");
            return resultTuples;
        }
    }
}
