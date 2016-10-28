using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class OperatorNode
    {
        public string ID { get; }

        #region Replicas Field
        // only gets the stubs when needed (when Replicas field is needed)
        private IList<string> addresses;
        private IList<IReplica> replicas;
        public IList<IReplica> Replicas
        {
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
            Console.WriteLine($"Operator {ID} initialized with {(Replicas.Count)} replicas.");
        }

        #region PuppetMaster's Commands
        public void Start()
        {
            if (Replicas != null)
                foreach (IReplica irep in Replicas)
                    irep.Start();
        }
        public void Interval(int mills)
        {
            if (Replicas != null)
                foreach (IReplica irep in Replicas)  // TODO - what if one of the interval requests gets lost. All replicas will be sleeping but that one will be processing
                    irep.Interval(mills);
        }
        public void Status()
        {
            if( Replicas != null)
            {
                foreach (IReplica irep in Replicas)
                    irep.Status();
            }
            else
            {
                Console.WriteLine("Status ain't doin' shit");
            }
        }
        #endregion
    }
}
