using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PuppetMaster
{ 
    class OperatorNode
    {
        public string ID { get; }

        #region Replicas Field
        // only gets the stubs when needed (when Replicas field is needed)
        private IList<string> addresses;
        private IList<IReplica> replicas;
        public IList<IReplica> Replicas {
            get
            {
                if (replicas == null)
                {
                    replicas = addresses.Select((address) => Helper.GetStub<IReplica>(address)).ToList();
                }
                return replicas;
            }
        }
        #endregion Replicas Field

        public OperatorNode(string ID, IList<string> addresses)
        {
            this.ID = ID;
            this.addresses = addresses;
        }

        #region PuppetMaster's Commands
        public void Start()
        {
            foreach(IReplica irep in Replicas)
            {
                irep.Start();
            }
        }
        public void Interval(int mills)
        {
            // TODO - what if one of the interval requests gets lost. All replicas will be sleeping but that one will be processing
            foreach (IReplica irep in Replicas)
            {
                irep.Interval(mills);
            }
        }
        public void Status()
        {
            foreach (IReplica irep in Replicas)
            {
                irep.Status();
            }
        }
        #endregion
    }

    class PMLoggerService : MarshalByRefObject, ILogger
    {
        private List<Record> eventsBuffer = new List<Record>();

        public PMLoggerService()
        {
            // starts a dedicated thread that from time to time empties the buffer
            Thread t = new Thread(FlushEventBuffer);
            t.Start();
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
                    // TODO - have to change the method when we have a GUI 
                    Console.WriteLine(s.ToString());
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
            lock (this)
            {
                eventsBuffer.Add(record);
                Monitor.Pulse(this);
            }
        }
    }

    class PuppetMaster
    {
        public const int PM_SERVICE_PORT = 10001;
        public const string PM_SERVICE_URL = "tcp://localhost:100001/PMLogger"; // mudar de acordo o necessário

        private PMLoggerService pmLogger = null;
        public IDictionary<string, ACommand> allCommands;
        public IDictionary<string, OperatorNode> nodes = new Dictionary<string, OperatorNode>(); 
        public bool fullLogging = false;
        public Semantic semantic;
        
        public PuppetMaster()
        {
            allCommands = new Dictionary<string, ACommand>
            {
                { "start" , new StartCommand(this) },
                { "interval", new IntervalCommand(this) },
                { "status", new StatusCommand(this) }
            };
            InitEventLogging();
        }

        #region Initialization
        public void ReadAndInitializeSystem(string config)
        {
            IDictionary<string, OperatorInfo> operators = new Dictionary<string, OperatorInfo>();

            //remove comments
            var commentRegex = new Regex(@"%[^\n]*", RegexOptions.IgnoreCase);
            config = commentRegex.Replace(config, "");


            var logRegex = new Regex(@"LoggingLevel\s+(?<level>(full|light))", RegexOptions.IgnoreCase);
            var logMatch = logRegex.Match(config);
            if (logMatch.Success) {
                config.Remove(logMatch.Index, logMatch.Length);
                if (logMatch.Groups["level"].Value.ToLower() == "full")
                {
                    fullLogging = true;
                }
            }


            var semanticRegex = new Regex(@"Semantics\s+(?<sem>(at-most-once|at-least-once|exactly-once))", RegexOptions.IgnoreCase);
            var semMatch = semanticRegex.Match(config);
            

            if (semMatch.Success)
            {
                config.Remove(semMatch.Index, semMatch.Length);
                var sem = semMatch.Groups["sem"].Value.ToLower();
                if (sem == "at-most-once")
                {
                    semantic = Semantic.AtMostOnce;
                }
                else if (sem == "exactly-once")
                {
                    semantic = Semantic.ExactlyOnce;
                }
                else
                {
                    semantic = Semantic.AtLeastOnce;
                }
            }


            string sourcesPattern = @"\s*(?<name>\w+)\s+INPUT_OPS\s+(?<sources>([a-zA-Z0-9.:/_\\]+|\s*,\s*)+)";
            string repPattern = @"\s+REP_FACT\s+(?<rep_fact>\d+)\s+ROUTING\s+(?<routing>(random|primary|hashing))(\((?<routing_arg>\d+)\))?";
            string addPattern = @"\s+ADDRESS\s+(?<addresses>([a-zA-Z0-9.:/_]+|\s*,\s*)+)";
            string opPattern = @"\s+OPERATOR_SPEC\s+(?<function>(\w+))\s+(?<function_args>(\w+|\s*,\s*|(""[^""\n]*""))+)";
            Regex opRegex = new Regex(sourcesPattern + repPattern + addPattern + opPattern, RegexOptions.IgnoreCase);

            var ops = opRegex.Matches(config);
            foreach (Match op in ops) {
                var sources = op.Groups["sources"].Value.Split(',');
                var addresses = op.Groups["addresses"].Value.Split(',');
                var functionArgs = op.Groups["function_args"].Value.Split(',');
                

                var hashingArg = op.Groups["routing_arg"].Success ? Int32.Parse(op.Groups["routing_arg"].Value) : -1;
                var stratString = op.Groups["routing"].Value.Trim().ToLower();
                var strat =  stratString == "random" ? RoutingStrategy.Random : stratString == "hashing" ? RoutingStrategy.Hashing : RoutingStrategy.Primary;
                var newOp = new OperatorInfo
                {
                    ID = op.Groups["name"].Value.Trim(),
                    MasterURL = PM_SERVICE_URL,
                    InputOperators = sources.Select((x) => x.Trim()).ToList(),
                    ReplicationFactor = Int32.Parse(op.Groups["rep_fact"].Value),
                    RtStrategy = strat,
                    Addresses = addresses.Select((x) => x.Trim()).ToList(),
                    OperatorFunction = op.Groups["function"].Value.Trim().Replace("\"", ""),
                    OperatorFunctionArgs = functionArgs.Select((x) => x.Trim().Replace("\"", "")).ToList(),
                    HashingArg = hashingArg,
                    Semantic = semantic,
                    ShouldNotify = fullLogging
                };
                
                if (assert(newOp))
                {
                    operators[newOp.ID] = newOp;
                    nodes.Add(newOp.ID, new OperatorNode(newOp.ID, newOp.Addresses));
                    Console.WriteLine($"Operator {newOp.ID} successfully parsed.");
                }

                // remove from the original string
                config.Remove(op.Index, op.Length);
            }
            


            foreach (var op in operators.Values)
            {
                // query to search all operators and match inputs to destinations
                op.OutputOperators = operators.Where((x) => x.Value.InputOperators.Contains(op.ID))
                    .Select((x) => 
                    new DestinationInfo
                    {
                        ID = x.Key,
                        Addresses = x.Value.Addresses,
                        ReplicationFactor = x.Value.ReplicationFactor,
                        RtStrategy = x.Value.RtStrategy
                    }).ToList();
                // those inputs that do not match any operator name are considered file inputs
                op.InputFiles = op.InputOperators.Where((x) => !operators.Keys.Contains(x)).ToList();
            }

            CreateAllProcesses(operators.Values);

        }
        public void InitEventLogging() {

            TcpChannel channel = new TcpChannel(PM_SERVICE_PORT);
            ChannelServices.RegisterChannel(channel, false);
            pmLogger = new PMLoggerService();
            RemotingServices.Marshal(pmLogger, "PMLogger");
        }

        public async void ExecuteNextCommand(StringReader reader)
        {
            var commandRegex = new Regex(@"^[ \t]*(?<command>\w+)(?<args>([ \t]+\w+)*)", RegexOptions.IgnoreCase);
            var line = reader.ReadLine();
            var done = false;
            while (!done && (line = reader.ReadLine()) != null )
            {

                var match = commandRegex.Match(line);
                if (!match.Success) continue;
                var command = match.Groups["command"].Value;
                if (!allCommands.ContainsKey(command)) continue;

                // if it is a valid command
                string[] args;
                var argsMatch = match.Groups["args"];
                if (argsMatch.Success && !String.IsNullOrWhiteSpace(argsMatch.Value))
                {
                    var argsString = argsMatch.Value.Trim();
                    args = argsString.Split(null).Select((x) => x.Trim()).ToArray();
                }
                else
                {
                    args = new string[0];
                }
                Console.WriteLine("Executing: " + match.Value);

                pmLogger.Notify((new Record(match.Value, DateTime.Now)));
                await Task.Run(()=>allCommands[command].execute(args));
                done = true;
            }
        }

        public void ExecuteCommands(string commands)
        {
            StringReader reader = new StringReader(commands);
            while (reader.Peek() != -1) ExecuteNextCommand(reader);
            reader.Close();
        }
        public string Serialize(OperatorInfo info, string address)
        {
            var rep = new ReplicaCreationInfo
            {
                Operator = info,
                Address = address
            };
            TextWriter tw = new StringWriter();
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(rep.GetType());
            x.Serialize(tw, rep);
            return tw.ToString();
        }
        public bool assert(OperatorInfo op)
        {
            Dictionary<string, int> functions = new Dictionary<string, int>()
            {
                { "DUP", 0 },
                { "FILTER", 3 },
                { "COUNT", 0 },
                { "UNIQ", 1 },
                { "CUSTOM", 3 }
            };
            string[] validOperators = new string[] { "=", ">", "<", ">=", "<=" };
            if (string.IsNullOrWhiteSpace(op.ID))
            {
                Console.WriteLine("Error while parsing config file. Operator ID cannot be empty.");
                return false;
            }
            if (op.InputOperators == null || op.InputOperators.Count == 0)
            {
                Console.WriteLine($"Error while parsing config file. Operator {op.ID} doesn't have  inputs.");
                return false;
            }
            if (op.ReplicationFactor <= 0)
            {
                Console.WriteLine($"Error while parsing config file. Operator {op.ID} doesn't have a replication factor greater than 0.");
                return false;
            }
            if (op.Addresses == null || op.Addresses.Count != op.ReplicationFactor)
            {
                Console.WriteLine($"Error while parsing config file. Operator {op.ID} has {op.Addresses.Count} addresses, expected {op.ReplicationFactor}.");
                return false;
            }
            if (!functions.Keys.Contains(op.OperatorFunction.ToUpper()))
            {
                Console.WriteLine($"Error while parsing config file. Operator {op.ID} doesn't have a valid function. Match: {op.OperatorFunction}. Expected one of {String.Join(",", functions.Keys)}.");
                return false;
            }
            var count = op.OperatorFunctionArgs?.Count ?? 0;
            if (count != functions[op.OperatorFunction.ToUpper()])
            {
                Console.WriteLine($"Error while parsing config file. Operator {op.ID} doesn't have correct amount of function arguments. Match: {String.Join(",", op.OperatorFunctionArgs)} ({op.OperatorFunctionArgs.Count} args). Expected {functions[op.OperatorFunction.ToUpper()]}.");
                return false;
            }
            if (op.RtStrategy == RoutingStrategy.Hashing && op.HashingArg < 0)
            {
                Console.WriteLine($"Error while parsing config file at Operator {op.ID}. Hashing requires an argument >= 0");
                return false;
            }
            if (op.OperatorFunction.ToUpper() == "FILTER" && !validOperators.Contains(op.OperatorFunctionArgs[1]))
            {
                Console.WriteLine($"Error while parsing config file. Operator {op.ID} doesn't have a valid function. Match: {op.OperatorFunctionArgs[1]}. Expected one of {String.Join(",", validOperators)}.");
            }
            return true;
        }

        private  void CreateProcessAt(string addr, OperatorInfo info)
        {
            try
            {
                Regex addrRegex = new Regex(@"tcp://(?<host>(\w|\.)+):(?<port>(\d+))(/\w*)?", RegexOptions.IgnoreCase);
                var match = addrRegex.Match(addr);
                if (!match.Groups["host"].Success)
                {
                    Console.WriteLine($"URL ({addr}) malformed. Unable to create process.");
                    return;
                }
                TcpClient client = new TcpClient(match.Groups["host"].Value, 10000);

                NetworkStream ns = client.GetStream();
                byte[] arg = Encoding.ASCII.GetBytes(Serialize(info, addr) + "\0");
                byte[] response = new byte[4];
                try
                {
                    // send request
                    ns.Write(arg, 0, arg.Length);

                    // receive reply
                    ns.Read(response, 0, response.Length);
                    
                    //close connection
                    ns.Close();
                    client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to create process for replica at {addr}. Exception: {e.ToString()}");
                }
                var pcsResponse = BitConverter.ToInt32(response, 0);
                if (pcsResponse == -1)
                {
                    throw new Exception("PCS replied with an error.");
                } else
                {
                    Console.WriteLine($"Successfuly created {info.ID} at {addr}. PID: {pcsResponse}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to create process for replica at {addr}. Exception: {e.ToString()}");
            }
        }
        public void CreateAllProcesses(ICollection<OperatorInfo> ops)
        {
            foreach (var op in ops)
            {
                foreach (var addr in op.Addresses)
                {
                    CreateProcessAt(addr, op);
                }
            }
        }
        #endregion Initialization

        #region PuppetMaster's commands
        public void Start(string opId)
        {
            try
            {
                nodes[opId].Start();
            } catch (KeyNotFoundException knfe)
            {
                Console.WriteLine($"Puppet master could not find operator {opId}");
            }
        }
        public void Interval(string opId, int x_mls)
        {
            try
            {
                nodes[opId].Interval(x_mls);
            } catch (KeyNotFoundException knfe)
            {
                Console.WriteLine($"Puppet master could not find operator {opId}");
            }
        }
        public void Status() {
            foreach(KeyValuePair<string, OperatorNode> pair in nodes)
                pair.Value.Status();
        }
        #endregion
    }
}
