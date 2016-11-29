using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{   
    class PMLoggerService : MarshalByRefObject, ILogger
    {
        public static MainForm form;
        private List<Record> eventsBuffer = new List<Record>();
        public IDictionary<string, Queue<Record>> latestRecordTimes;
        public IDictionary<string, int> totals;
        public Timer throughputUpdater;

        public PMLoggerService()
        {
            // starts a dedicated thread that from time to time empties the buffer
            Thread t = new Thread(FlushEventBuffer);
            t.Start();
            latestRecordTimes = new Dictionary<string, Queue<Record>>();
            totals = new Dictionary<string, int>();
            throughputUpdater = new System.Threading.Timer((e) =>
            {
                UpdateThroughputs();
            }, null, 0, (int) TimeSpan.FromSeconds(2).TotalMilliseconds);
        }

        private void FlushEventBuffer()
        {
            lock (this)
            {
                while (eventsBuffer.Count == 0)
                    Monitor.Wait(this);

                // might be src of bug

                int eventsLeft = eventsBuffer.Count;
                eventsBuffer.Sort((r1, r2) => r1.CompareTo(r2));
                foreach (Record s in eventsBuffer)
                {
                    //Console.WriteLine(s.ToString());
                    if(form != null)
                    {
                        form.BeginInvoke(new MainForm.UpdateFormDelegate(form.LogEvent), new object[] { s.ToString() });
                    }

                }
                eventsBuffer.Clear();
                Monitor.Pulse(this);
            }

            Thread.Sleep(10);
            FlushEventBuffer();
        }

        public override object InitializeLifetimeService() { return (null); }
        public void Notify(Record record)
        {
            lock (latestRecordTimes)
            {
                if (!latestRecordTimes.ContainsKey(record.Owner))
                {
                    latestRecordTimes[record.Owner] = new Queue<Record>();
                    totals[record.Owner] = 0;
                }

                var queue = latestRecordTimes[record.Owner];
                var total = ++totals[record.Owner];
                var r = new Record(record.Type, record.Owner, record.Content, DateTime.Now);
                queue.Enqueue(r);
            }
            lock (this)
            {
                eventsBuffer.Add(record);
                Monitor.Pulse(this);
            }
        }

        private void UpdateThroughputs()
        {
            lock (latestRecordTimes)
            {
                foreach (var owner in latestRecordTimes.Keys)
                {
                    
                    var queue = latestRecordTimes[owner];
                    if (queue.Count == 0)
                        continue;
                    var firstVal = queue.Peek();
                    var now = DateTime.Now;
                    while ((now - firstVal.Timestamp).TotalMilliseconds > 1000)
                    {
                        queue.Dequeue();
                        if (queue.Count == 0)
                            break;
                        firstVal = queue.Peek();
                    }
                    Console.WriteLine($"Throughput of ${owner} is ${queue.Count} per second");
                }
            }

            
        }
    }
}
