using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes.Reliable_multicast
{
    public class ReliableMulticastSender : IMulticastReceiver
    {
        private Dictionary<int, bool> seenMessages = new Dictionary<int, bool>();
        private Random rand = new Random();
        public void Send<T>(T message, IEnumerable<IMulticastReceiver> receivers, IMulticastReceiver sender = null, int? id = null)
        {
            var tasks = new List<Task>();
            int messageId = id ?? rand.Next();
            while (seenMessages.ContainsKey(messageId)) messageId = rand.Next();
            if (seenMessages.ContainsKey(messageId) && seenMessages[messageId])
                return;
            foreach (var r in receivers)
            {
                if (r == sender)
                    continue;
                var task = Task.Run(() => {
                    bool sent = false;
                    while (!sent)
                    {
                        try
                        {
                            r.Send(message, receivers, this, messageId);
                            sent = true;
                        }
                        catch (RemotingTimeoutException e)
                        {

                        }
                    }
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
