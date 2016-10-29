using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public static class CustomFunctions
    {
        public static IList<string>[] reverse(IList<string> tuple)
        {
            return new IList<string>[] { tuple.Reverse().ToList() };
        }
        public static IList<string>[] dup(IList<string> tuple)
        {
            return new IList<string>[] { tuple, tuple };
        }
    }
}
