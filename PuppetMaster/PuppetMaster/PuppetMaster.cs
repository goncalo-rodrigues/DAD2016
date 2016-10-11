using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuppetMaster
{
    enum RoutingStrategy
    {
        Primary = 0,
        Random = 1,
        Hashing = 2
    }

    enum Semantic
    {
        AtLeastOnce = 0,
        AtMostOnce = 1,
        ExactlyOnce = 2
    }
    class Operator
    {
        public string ID { get;  set; }
        public int ReplicationFactor { get; set; }
        public RoutingStrategy RtStrategy { get; set; }
        public IList<string> InputOperators { get; set; }
        public IList<string> Addresses { get; set; } // of the multiple replicas
        public string OperatorFunction { get; set; }
        public IList<string> OperatorFunctionArgs { get; set; }
        public int HashingArg { get; set; } // only applicable if routingstrat == hashing
    }
    class PuppetMaster
    {
        public IList<Operator> operators = new List<Operator>();
        public static bool fullLogging = false;
        public static Semantic semantic;
        public static void read(string config)
        {
            string sourcesPattern = @"\s*(?<name>\w+)\s+INPUT_OPS(?<sources>([a-zA-Z0-9.:,/_]|\s)+)";
            string repPattern = @"\s+REP_FACT\s+(?<rep_fact>\d+)\s+ROUTING\s+(?<routing>(random|primary|hashing))(\((?<routing_arg>\d+)\))?";
            string addPattern = @"\s+ADDRESS\s+(?<addresses>([a-zA-Z0-9.:,/_]|\s)+)";
            string opPattern = @"\s+OPERATOR_SPEC\s+(?<function>(\w+))\s+(?<function_args>(\w|,| |(""[^""]*""))+)";
            Regex opRegex = new Regex(sourcesPattern + repPattern + addPattern + opPattern, RegexOptions.IgnoreCase);

            var ops = opRegex.Matches(config);
            foreach (Match op in ops) {
                var sources = op.Groups["sources"].Value.Split(',');
                var addresses = op.Groups["addresses"].Value.Split(',');
                var functionArgs = op.Groups["function_args"].Value.Split(',');
                addresses.AsParallel().ForAll((s) => s.Trim());
                functionArgs.AsParallel().ForAll((s) => s.Trim());

                var stratString = op.Groups["routing"].Value.Trim().ToLower();
                var strat =  stratString == "random" ? RoutingStrategy.Random : stratString == "hashing" ? RoutingStrategy.Hashing : RoutingStrategy.Primary;
                var newOp = new Operator
                {
                    ID = op.Groups["name"].Value.Trim(),
                    InputOperators = sources.Select((x) => x.Trim()).ToList(),
                    ReplicationFactor = Int32.Parse(op.Groups["rep_fact"].Value),
                    RtStrategy = strat,
                    Addresses = addresses.Select((x) => x.Trim()).ToList(),
                    OperatorFunction = op.Groups["function"].Value.Trim(),
                    OperatorFunctionArgs = functionArgs.Select((x) => x.Trim()).ToList(),
                    HashingArg = Int32.Parse(op.Groups["routing_arg"].Value)
                };
                //operators.Add(newOp);
            }

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
            } else if (sem == "exactly-once")
            {
                semantic = Semantic.ExactlyOnce;
            } else
            {
                semantic = Semantic.AtLeastOnce;
            }
        }
    }
}
