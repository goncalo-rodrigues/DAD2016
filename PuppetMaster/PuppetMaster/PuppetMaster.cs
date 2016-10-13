using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuppetMaster
{ 
    class OperatorNode
    {
        public string ID { get; set; }
        public List<string> Addresses { get; set; }
    }
    class PuppetMaster
    {
        
        public static IDictionary<string, OperatorNode> nodes = new Dictionary<string, OperatorNode>(); 
        public static bool fullLogging = false;
        public static Semantic semantic;
        public static void ReadAndInitializeSystem(string config)
        {
            IDictionary<string, OperatorInfo> operators = new Dictionary<string, OperatorInfo>();
            var logRegex = new Regex(@"LoggingLevel\s+(?<level>(full|light))", RegexOptions.IgnoreCase);
            if (logRegex.Match(config).Groups["level"].Value.ToLower() == "full")
            {
                fullLogging = true;
            }

            var semanticRegex = new Regex(@"Semantics\s+(?<sem>(at-most-once|at-least-once|exactly-once))", RegexOptions.IgnoreCase);
            var sem = semanticRegex.Match(config).Groups["sem"].Value.ToLower();
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
                    nodes.Add(newOp.ID, new OperatorNode { ID = newOp.ID, Addresses = newOp.Addresses });
                    Console.WriteLine($"Operator {newOp.ID} successfully parsed.");
                }
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

        public static string Serialize(OperatorInfo info, string address)
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

        public static bool assert(OperatorInfo op)
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

        private static void CreateProcessAt(string addr, OperatorInfo info)
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
                byte[] arg = Encoding.ASCII.GetBytes(Serialize(info, addr));
                byte[] response = new byte[4];
                try
                {
                    ns.Write(arg, 0, arg.Length);
                    ns.Close();
                    client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to create process for replica at {addr}. Exception: {e.ToString()}");
                }
                


            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to create process for replica at {addr}. Exception: {e.ToString()}");
            }
        }
        public static void CreateAllProcesses(ICollection<OperatorInfo> ops)
        {
            foreach (var op in ops)
            {
                foreach (var addr in op.Addresses)
                {
                    CreateProcessAt(addr, op);
                }
            }
        }
    }
}
