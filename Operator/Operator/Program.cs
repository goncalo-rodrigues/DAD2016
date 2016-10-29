using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    class Program
    {
        static void Main(string[] args)
        {
            //var a = Operations.GetCustomOperation(@"SharedTypes.dll", "SharedTypes.CustomFunctions", "reverse");
            // var b = Operations.GetCustomOperation(@"C:\Users\Goncalo\Source\Repos\DAD2016\Operator\Operator\bin\Debug\SharedTypes.dll", "SharedTypes.CustomFunctions", "dup");
            //var c = a(new List<string> { "a", "b" });
            // var d = b(new List<string> { "a", "b" });
            if (args.Length < 1)
            {
                Console.WriteLine("Missing creation info argument.");
                loop();
            }

            TextReader tr = new StringReader(args[0]);
            ReplicaCreationInfo rep = null;
            try
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(ReplicaCreationInfo));
                rep = (ReplicaCreationInfo)x.Deserialize(tr);
            } catch (Exception e)
            {
                Console.WriteLine($"Creation info XML has a wrong format. Exception: {e.Message}");
                throw;
            } finally
            {
                tr.Close();
            }

            if (rep == null) loop();
            var info = rep.Operator;
            Console.WriteLine($"Successfully initiated this replica of {info.ID}.");
            string address = rep.Address;
            info.Addresses.Remove(address);


            Replica replica = new Replica(rep);
            Operations.ReplicaInstance = replica;

            //loop();

            // get port from address 
            Regex portRegex = new Regex(@"tcp://(\w|\.)+:(?<port>(\d+))(/(?<name>\w*))?", RegexOptions.IgnoreCase);
            var match = portRegex.Match(address);
            if (!match.Groups["port"].Success || !match.Groups["name"].Success) { 
                Console.WriteLine($"URL ({address}) malformed. Unable to create process.");
                return;
            }
            var port = Int32.Parse(match.Groups["port"].Value);
            var name = match.Groups["name"].Value;
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(replica, name, typeof(Replica));
            Console.WriteLine($"Listening at {address}. Press Enter to exit.");
            Console.ReadLine();
            
        }

        static void loop()
        {
        }
    }
}
