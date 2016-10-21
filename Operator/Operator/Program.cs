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
            //args = new string[1];
            var s = 
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<ReplicaCreationInfo xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Address>tcp://localhost:12001/op</Address>
  <Operator>
    <ID>OP1</ID>
    <MasterURL>tcp://localhost:100001/PMLogger</MasterURL>
    <ReplicationFactor>1</ReplicationFactor>
    <RtStrategy>Hashing</RtStrategy>
    <InputOperators>
      <string>tweeters.data</string>
    </InputOperators>
    <InputFiles>
      <string>tweeters.data</string>
    </InputFiles>
    <OutputOperators>
      <DestinationInfo>
        <ID>OP2</ID>
        <field_id>0</field_id>
        <ReplicationFactor>1</ReplicationFactor>
        <RtStrategy>Random</RtStrategy>
        <Addresses>
          <string>tcp://localhost:11001/op</string>
        </Addresses>
      </DestinationInfo>
    </OutputOperators>
    <Addresses>
      <string>tcp://localhost:11000/op</string>
    </Addresses>
    <OperatorFunction>FILTER</OperatorFunction>
    <OperatorFunctionArgs>
      <string>3</string>
      <string>=</string>
      <string>www.tecnico.ulisboa.pt</string>
    </OperatorFunctionArgs>
    <HashingArg>1</HashingArg>
    <Semantic>ExactlyOnce</Semantic>
    <ShouldNotify>true</ShouldNotify>
  </Operator>
</ReplicaCreationInfo>";
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
            string address = rep.Address;
            info.Addresses.Remove(address);


            Replica replica = new Replica(rep);
            Operations.ReplicaInstance = replica;

            loop();

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
            Console.ReadLine();
        }

        static void loop()
        {
            Thread.Sleep(10000);
        }
    }
}
