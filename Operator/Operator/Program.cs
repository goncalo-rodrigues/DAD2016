using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    class Program
    {
        static void Main(string[] args)
        {
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
            } finally
            {
                tr.Close();
            }

            if (rep == null) loop();
            var info = rep.Operator;
            string address = rep.Address;
            address = args[1];
            info.Addresses.Remove(address);
            

            Replica replica = new Replica(rep);
            Operations.ReplicaInstance = replica;

            loop();
            //create port and listen
        }

        static void loop()
        {
            Thread.Sleep(100000);
        }
    }
}
