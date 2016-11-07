using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public interface IReplica
    {
        void ProcessAndForward(CTuple tuple);
        int IncrementCount();

        void Start();
        void Interval(int mils);
        void Status();
        void Ping();
        void Kill();
        void Freeze();
        void Unfreeze();
    }
}
