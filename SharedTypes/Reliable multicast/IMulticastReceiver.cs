using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes.Reliable_multicast
{
    public interface IMulticastReceiver
    {
        void Send<T>(T message, IEnumerable<IMulticastReceiver> receivers, IMulticastReceiver sender, int? id);
    }
}
