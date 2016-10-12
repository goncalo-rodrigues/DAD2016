using SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                return;
            }

            TextReader tr = new StringReader(args[0]);
            OperatorCreationInfo info = null;
            try
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(OperatorCreationInfo));
                info = (OperatorCreationInfo)x.Deserialize(tr);
            } catch (Exception e)
            {
                Console.WriteLine($"Creation info XML has a wrong format. Exception: {e.Message}");
            } finally
            {
                tr.Close();
            }

            if (info == null) return;

            string address = "";
            if (args.Length < 2)
            {
                if (info.Addresses.Count == 1)
                {
                    address = info.Addresses[0];
                    info.Addresses.Clear();
                } else
                {
                    Console.WriteLine("Missing address argument.");
                    return;
                }
            } else
            {
                address = args[1];
                var success = info.Addresses.Remove(address);
                if (!success)
                {
                    Console.WriteLine("Address argument does not match with any of the replica addresses");
                    return;
                }
            }

            Replica replica = new Replica(info);

            //create port and listen
        }
    }
}
