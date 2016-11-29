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
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Collections;
using System.Runtime.Serialization.Formatters;

namespace PuppetMaster
{ 

    public class PuppetMaster
    {
        const int SWP_NOZORDER = 0x4;
        const int SWP_NOACTIVATE = 0x10;
        [DllImport("kernel32")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, int flags);

        public delegate void PuppetMasterAsyncIntDelegate(int i);

        internal void ExecuteCommand(string command, string[] args = null)
        {
            if (!String.IsNullOrEmpty(command))
            {
                allCommands[command].Execute(args);
            }
        }

        public delegate void PuppetMasterAsyncVoidDelegate();

        public String commandList;
        public const int PM_SERVICE_PORT = 10001;
        public string PM_SERVICE_URL = $"tcp://localhost:{PM_SERVICE_PORT}/PMLogger";

        private PMLoggerService pmLogger = null;
        public IDictionary<string, ACommand> allCommands;
        public IDictionary<string, OperatorNode> nodes = new Dictionary<string, OperatorNode>(); 
        public bool fullLogging = false;
        public Semantic semantic;
        public string commandsToBeExecuted = null;
       
        public PuppetMaster()
        {
            allCommands = new Dictionary<string, ACommand>
            {
                { "start" , new StartCommand(this) },
                { "interval", new IntervalCommand(this) },
                { "status", new StatusCommand(this) },
                { "crash", new CrashCommand(this) },
                { "freeze", new FreezeCommand(this) },
                { "unfreeze", new UnfreezeCommand(this) },
                { "wait", new WaitCommand(this) }
            };
            // Configure windows position
            Console.Title = "PuppetMaster";
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var width = screen.Width;
            var height = screen.Height;
            SetWindowPosition(0, 0, width/2, height/3);

            InitEventLogging();
        }

        public ILogger getLogger() { return pmLogger; }

        #region Initialization
        public void ReadAndInitializeSystem(string config)
        {
            IDictionary<string, OperatorInfo> operators = new Dictionary<string, OperatorInfo>();

            //remove comments
            var commentRegex = new Regex(@"%[^\n]*\n?", RegexOptions.IgnoreCase);
            config = commentRegex.Replace(config, "");

            // find log level (full or light)
            var logRegex = new Regex(@"LoggingLevel\s+(?<level>(full|light))\s*", RegexOptions.IgnoreCase);
            var logMatch = logRegex.Match(config);
            if (logMatch.Success) {
                config = config.Remove(logMatch.Index, logMatch.Length);
                if (logMatch.Groups["level"].Value.ToLower() == "full")
                {
                    fullLogging = true;
                }
            }

            // find semantic
            var semanticRegex = new Regex(@"Semantics\s+(?<sem>(at-most-once|at-least-once|exactly-once))\s*", RegexOptions.IgnoreCase);
            var semMatch = semanticRegex.Match(config);
            

            if (semMatch.Success)
            {
                config = config.Remove(semMatch.Index, semMatch.Length);
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

            // find operators 
            const string sourcesPattern = @"\s*(?<name>\w+)\s+INPUT OPS\s+(?<sources>([a-zA-Z0-9.:/_\\]+|\s*,\s*)+)";
            const string repPattern = @"\s+REP FACT\s+(?<rep_fact>\d+)\s+ROUTING\s+(?<routing>(random|primary|hashing))(\((?<routing_arg>\d+)\))?";
            const string addPattern = @"\s+ADDRESS\s+(?<addresses>([a-zA-Z0-9.:/_]+|\s*,\s*)+)";
            const string opPattern = @"\s+OPERATOR SPEC\s+((?<function>(count|dup))|(?<function>(uniq|custom|filter))\s+(?<function_args>([\w=><.\\/:-]+|\s*,\s*|(""[^""\n]*""))+))\s*";

            Regex opRegex = new Regex(sourcesPattern + repPattern + addPattern + opPattern, RegexOptions.IgnoreCase);

            // this variable is used to properly remove the matches in the original text
            var totalLengthRemoved = 0;

            // actually match
            var ops = opRegex.Matches(config);
            foreach (Match op in ops) {
                // get all variables found in the match, carefully removing whitespace
                var sources = op.Groups["sources"].Value.Split(',');
                var addresses = op.Groups["addresses"].Value.Split(',');
                var functionArgs = op.Groups["function_args"].Success ? op.Groups["function_args"].Value.Split(',') : new string[0];
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
                    OperatorFunction = op.Groups["function"].Value.Trim(),
                    OperatorFunctionArgs = functionArgs.Select((x) => x.Trim().Trim('"')).ToList(),
                    HashingArg = hashingArg,
                    Semantic = semantic,
                    ShouldNotify = fullLogging

                };
                
                // check if everything is ok
                if (Assert(newOp))
                {
                    operators[newOp.ID] = newOp;
                    nodes.Add(newOp.ID, new OperatorNode(newOp.ID, newOp.Addresses));
                    Console.WriteLine($"Operator {newOp.ID} successfully parsed.");
                }

                // remove from the original string
                config = config.Remove(op.Index - totalLengthRemoved, op.Length);
                totalLengthRemoved += op.Length;
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
                        RtStrategy = x.Value.RtStrategy,
                        HashingArg = x.Value.HashingArg
                    }).ToList();
                // those inputs that do not match any operator name are considered file inputs
                op.InputFiles = op.InputOperators.Where((x) => !operators.Keys.Contains(x)).ToList();
                TempInputReplicas = op.InputOperators.Where((x) => operators.Keys.Contains(x)).ToList();

                foreach (string s in TempInputReplicas)
                {
                   // try { 
                    if (op.InputReplicas != null)
                        op.InputReplicas.AddRange(operators[s].Addresses);
                        Console.WriteLine("OP ID: " + s + " url: " + operators[s].Addresses);
                   /* }catch (NullReferenceException e) {
                    }*/
                 
                }
            }

           


                // after all parsing, start creating the processes
                CreateAllProcesses(operators.Values);
            commandsToBeExecuted = config;

            Task.Run(async () =>
            {
                while (true)
                {
                    var success = await ExecuteNextCommand(Console.In);
                    if (!success)
                    {
                        Console.WriteLine("Unknown command.");
                    }
                }
                    
            });
        }

        public void InitEventLogging() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = PM_SERVICE_PORT;
            //timeout for puppetMaster responses
            props["timeout"] = 500; // in milliseconds

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, false);
            pmLogger = new PMLoggerService();
            RemotingServices.Marshal(pmLogger, "PMLogger");
            Console.WriteLine("Logger was successfully initialized");

          
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
        public bool Assert(OperatorInfo op)
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

        /// <summary>
        /// Communicates with the PCS to create a process at a given address
        /// </summary>
        /// <param name="addr">The address of the replica</param>
        /// <param name="info">The replica information</param>
        private  void CreateProcessAt(string addr, OperatorInfo info)
        {
            const int PCS_PORT = 10000;
            try
            {
                Regex addrRegex = new Regex(@"tcp://(?<host>(\w|\.)+):(?<port>(\d+))(/\w*)?", RegexOptions.IgnoreCase);
                var match = addrRegex.Match(addr);
                if (!match.Groups["host"].Success)
                {
                    Console.WriteLine($"URL ({addr}) malformed. Unable to create process.");
                    return;
                }
                TcpClient client = new TcpClient(match.Groups["host"].Value, PCS_PORT);

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
            List<Task> tasks = new List<Task>();
            if(ops != null)
            foreach (var op in ops)
            {
                    if(op.Addresses != null)
                foreach (var addr in op.Addresses)
                {
                    tasks.Add(Task.Run(() => CreateProcessAt(addr, op)));
                }
            }
            Task.WaitAll(tasks.ToArray());
        }
        #endregion Initialization
        #region Command Parsing
        /// <summary>
        /// Executes the next command given a stream (Reader). Ignores whitespace. The command is executed in a different thread.
        /// </summary>
        /// <param name="reader">The stream to search for the next command</param>
        /// <returns>False if no more commands exist.</returns>
        public async Task<bool> ExecuteNextCommand(TextReader reader)
        {
            var commandRegex = new Regex(@"^[ \t]*(?<command>\w+)(?<args>([ \t]+\w+)*)\s*$", RegexOptions.IgnoreCase);
            var success = false;
            var done = false;
            string line = null;
            while (!done && (line = reader.ReadLine()) != null)
            {
                var match = commandRegex.Match(line);
                if (!match.Success) continue;
                var command = match.Groups["command"].Value;
                if (!allCommands.ContainsKey(command.ToLower()))
                {
                    Console.WriteLine($"Ignoring unrecognized command ({line})");
                    continue;
                }
                success = true;
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

                await Task.Run(() => allCommands[command.ToLower()].Execute(args));
                done = true;
            }
            return success;
        }

        // Executes all commands sequentially
        public async void ExecuteCommands(string commands = null)
        {
            if (commands == null)
                commands = commandsToBeExecuted;
            using (var reader = new StringReader(commands))
            {
                while (reader.Peek() != -1) await ExecuteNextCommand(reader);
            }
        }
        #endregion Command Parsing
        #region PuppetMaster's commands
        public static void PuppetMasterCommandsIntAsyncCallBack(IAsyncResult ar)
        {
            PuppetMasterAsyncIntDelegate del = (PuppetMasterAsyncIntDelegate)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }
        public static void PuppetMasterCommandsVoidAsyncCallBack(IAsyncResult ar)
        {
            PuppetMasterAsyncVoidDelegate del = (PuppetMasterAsyncVoidDelegate)((AsyncResult)ar).AsyncDelegate;
            del.EndInvoke(ar);
            return;
        }

        public void Start(string opId)
        {
            try
            {
                if(nodes != null && nodes[opId] != null)
                {
                    PuppetMasterAsyncVoidDelegate remoteDel = new PuppetMasterAsyncVoidDelegate(nodes[opId].Start);
                    AsyncCallback puppetCallback = new AsyncCallback(PuppetMasterCommandsVoidAsyncCallBack);
                    remoteDel.BeginInvoke(puppetCallback, null);
                }
            } catch (KeyNotFoundException knfe)
            {
                Console.WriteLine($"Puppet master could not find operator {opId}");
            } catch (SocketException se)
            {
                Console.WriteLine($"Due to connection problems, puppet master couldn't invoke Start on operator {opId} ");
            }
        }
        public void Interval(string opId, int x_mls)
        {
            try
            {
                if (nodes != null && nodes[opId] != null)
                {
                    PuppetMasterAsyncIntDelegate remoteDel = new PuppetMasterAsyncIntDelegate(nodes[opId].Interval);
                     AsyncCallback puppetCallback = new AsyncCallback(PuppetMasterCommandsIntAsyncCallBack);
                    remoteDel.BeginInvoke(x_mls, puppetCallback, null);
                }
            } catch (KeyNotFoundException knfe)
            {
                Console.WriteLine($"Puppet master could not find operator {opId}");
            } catch (SocketException se)
            {
                Console.WriteLine($"Due to connection problems, puppet master couldn't invoke Interval on operator {opId} ");
            }
        }
        public void Status() {
            string opId = "";
            if (nodes != null)
                foreach (KeyValuePair<string, OperatorNode> pair in nodes)
                {
                    try
                    {
                        opId = pair.Key;
                        OperatorNode node = pair.Value;
                        PuppetMasterAsyncVoidDelegate remoteDel = new PuppetMasterAsyncVoidDelegate(node.Status);
                        AsyncCallback puppetCallback = new AsyncCallback(PuppetMasterCommandsVoidAsyncCallBack);
                        remoteDel.BeginInvoke(puppetCallback, null);
                    }
                    catch (NullReferenceException)
                    {
                        Console.WriteLine($"Due to unexpected problems puppet master couldn't invoke Status on operator {opId}.");
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine($"Due to connection problems puppet master couldn't invoke Status on operator {opId}.");
                    }
                }
        }

        public void Crash(string opID,  int index )
        {
            if (!String.IsNullOrEmpty(opID))
            {
                OperatorNode op = nodes[opID];
                if (index >= 0 && index < op.Replicas.Count)
                {
                    IReplica rep = op.Replicas[index];
                    rep.Kill();
                }
            }
        }
        public void Freeze(string opID, int index)
        {
            if (!String.IsNullOrEmpty(opID))
            {
                OperatorNode op = nodes[opID];
                if (index >= 0 && index < op.Replicas.Count)
                {
                    IReplica rep = op.Replicas[index];
                    rep.Freeze();
                }
            }
        }
        public void Unfreeze(string opID, int index)
        {
            if (!String.IsNullOrEmpty(opID))
            {
                OperatorNode op = nodes[opID];
                if (index >= 0 && index < op.Replicas.Count)
                {
                    IReplica rep = op.Replicas[index];
                    rep.Unfreeze();
                }
            }
        }
        public void Wait(int ms)
        {
            if (ms >= 0)
            {
                Console.WriteLine($"PupperMaster is pausing for {ms} seconds...");
                Thread.Sleep(ms);
                Console.WriteLine($"PupperMaster is leaving pause.");
            }
        }

        #endregion


        /*Just to configure windows position*/
        public static void SetWindowPosition(int x, int y, int width, int height)
        {
            SetWindowPos(Handle, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }
        public static IntPtr Handle
        {
            get
            {
                return GetConsoleWindow();
            }
        }

        public List<string> TempInputReplicas { get; private set; }
        /*Just to configure windows position - END*/
    }
}
