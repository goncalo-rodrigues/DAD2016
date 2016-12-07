using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    [Serializable]
    public class CustomFunctions
    {
        public IList<string>[] reverse(IList<string> tuple)
        {
            return new IList<string>[] { tuple.Reverse().ToList() };
        }
        public IList<string>[] dup(IList<string> tuple)
        {
            return new IList<string>[] { tuple, tuple };
        }
    }
}
