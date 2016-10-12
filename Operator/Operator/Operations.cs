using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class Operations
    {
        public static ProcessDelegate GetOperation(string operationName, IList<string> args = null)
        {
            if (operationName == "DUP")
            {
                return GetDupOperation();
            } else if (operationName == "UNIQ")
            {
                return GetUniqOperation(Int32.Parse(args[0]));
            } else if (operationName == "COUNT")
            {
                return GetCountOperation();
            } else if (operationName == "FILTER")
            {
                return GetFilterOperation(Int32.Parse(args[0]), args[1], args[2]);
            } else if (operationName == "CUSTOM")
            {
                return GetCustomOperation(args[0], args[1], args[2]);
            } else
            {
                return null;
            }
        }
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
