using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class Operations
    {
        public static ProcessDelegate GetDupOperation()
        {
            return new ProcessDelegate((x) => new IList<string>[] { x });
        }

        public static ProcessDelegate GetUniqOperation(int fieldNumber)
        {
            throw new NotImplementedException();
        }

        public static ProcessDelegate GetCountOperation()
        {
            throw new NotImplementedException();
        }

        public static ProcessDelegate GetFilterOperation(int fieldNumber, string comparator, string value)
        {
            throw new NotImplementedException();
        }

        public static ProcessDelegate GetCustomOperation(string dll, string className, string method)
        {
            throw new NotImplementedException();
        }
    }
}
