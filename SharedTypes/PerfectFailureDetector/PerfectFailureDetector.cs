using System;
using System.Collections.Generic;
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
        public int TIMEOUT_MILLIS = 2000;
        public int PING_PERIOD = 5000;
        public event NodeFailedEventHandler NodeFailed;
        private IDictionary<string, MonitoredNode> monitoredNodes = new Dictionary<string, MonitoredNode>();
        private Timer mainTimer;

        public PerfectFailureDetector()
        {
            mainTimer = new System.Threading.Timer((e) =>
            {
                foreach (var n in monitoredNodes.Values)
                {
                    lock (n)
                    {
                        if (n.Failed) continue;
                    }
                    
                    Task.Run(() =>
                    {
                        var success = Ping(n);
                        lock(n)
                        {
                            n.Failed = !success;
                            if (n.Failed)
                                NodeFailed?.Invoke(this, new NodeFailedEventArgs(n.Name, n.pingable));
                            else Console.WriteLine($"Successfully pinged {n.Name}");
                        }

                           
                    });
                }
                
            }, null, 0, PING_PERIOD);
        }
        public void StartMonitoringNewNode(string name, IPingable node)
        {
            monitoredNodes[name] = new MonitoredNode(name, node);
        }
       
        public bool IsAlive(string name)
        {
            return monitoredNodes[name].Failed;
        }

        private bool Ping(MonitoredNode node)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;
            var task = Task.Factory.StartNew(() =>
            {
                var success = false;
                while (!success && !cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        node.Ping();
                        success = true;
                    } catch (Exception e)
                    {
                        Console.WriteLine("fail ping: " + e.Message);
                    }
                    
                }
                Console.WriteLine("success ping");
            }, cancelToken);
            var result = task.Wait(TIMEOUT_MILLIS);
            if (!result) cancelTokenSource.Cancel();
            return result;
        }
    }

    
}
