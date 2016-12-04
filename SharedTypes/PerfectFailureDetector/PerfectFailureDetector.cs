using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedTypes.PerfectFailureDetector
{
    public class NodeFailedEventArgs
    {
        public string FailedNodeName { get; }
        public IPingable FailedNode { get; }
        public NodeFailedEventArgs(string name, IPingable failedNode)
        {
            this.FailedNodeName = name;
            this.FailedNode = failedNode;
        }
    }

    public class MonitoredNode : IPingable
    {
        public IPingable pingable;
        public bool Failed { get; set; } = false;
        public bool FirstTime { get; set; } = true;
        public string Name { get; set; }
        public MonitoredNode(string name, IPingable pingable)
        {
            this.pingable = pingable;
            this.Name = name;
        }
        public void Ping()
        {
            pingable.Ping();
        }
    }

    public delegate void NodeFailedEventHandler(object sender, NodeFailedEventArgs e);
    public class PerfectFailureDetector
    {
        public int TIMEOUT_MILLIS_FIRST_TIME = 2000;
        public int TIMEOUT_MILLIS = 500;
        public int PING_PERIOD = 5000;
        public event NodeFailedEventHandler NodeFailed;
        private IDictionary<string, MonitoredNode> monitoredNodes = new Dictionary<string, MonitoredNode>();
        private Timer mainTimer;

        public PerfectFailureDetector()
        {
            mainTimer = new System.Threading.Timer((e) =>
            {
                lock (monitoredNodes)
                {
                    foreach (var n in monitoredNodes.Values)
                    {
                        long timeOut;
                        lock (n)
                        {
                            timeOut = n.FirstTime ? TIMEOUT_MILLIS_FIRST_TIME : TIMEOUT_MILLIS;
                            n.FirstTime = false;
                            if (n.Failed) continue;
                        }

                        Task.Run(() =>
                        {
                            var success = Ping(n, timeOut);
                            lock (n)
                            {
                                n.Failed = !success;
                                if (n.Failed)
                                    NodeFailed?.Invoke(this, new NodeFailedEventArgs(n.Name, n.pingable));
                                //else Console.WriteLine($"Successfully pinged {n.Name}");
                            }


                        });
                    }
                }
                
                
            }, null, 0, PING_PERIOD);
        }
        public void StartMonitoringNewNode(string name, IPingable node)
        {
            lock (monitoredNodes)
            {
                monitoredNodes[name] = new MonitoredNode(name, node);
            }
        }
       
        public bool IsAlive(string name)
        {
            return !monitoredNodes[name].Failed;
        }

        private bool Ping(MonitoredNode node, long timeOut)
        {
            //var cancelTokenSource = new CancellationTokenSource();
            //var cancelToken = cancelTokenSource.Token;
            var task = Task.Run(() =>
            {
                var success = false;
                long millis = 0;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                while (!success && millis < timeOut)
                {
                    //Console.WriteLine("starting ping...");
                    try
                    {
                        var action = new Action(node.Ping);
                        var handler = action.BeginInvoke(null, null);

                        if (handler.AsyncWaitHandle.WaitOne((int)timeOut))
                        {
                            success = true;
                        }
                        else
                        {
                            Console.WriteLine("Ping time out");
                        }
                            
                        //node.Ping();
                        
                    } catch (Exception e)
                    {
                        Console.WriteLine("fail ping: " + e.Message);
                    }
                    millis = stopWatch.ElapsedMilliseconds;
                }
                
                //Console.WriteLine($"success ping, took {millis} ms. max is {timeOut}");
                return millis < timeOut;
            });
            var result = task.Result;
            
            return result;
        }
    }

    
}
