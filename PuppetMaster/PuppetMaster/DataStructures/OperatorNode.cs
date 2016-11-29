using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;


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
        }

        #region PuppetMaster's Commands
        public void Start()
        {
            if (Replicas != null)

                for (int i = 0; i < Replicas.Count; i++) {
                    if (Replicas[i] != null) {
                        Replicas[i].Start(i);
                    }
                }
              
        }
        public void Interval(int mills)
        {
            if (Replicas != null)
            {
                for (int i = 0; i < Replicas.Count; i++)
                {
                    // TODO - what if one of the interval requests gets lost. All replicas will be sleeping but that one will be processing
                    Replicas[i].Interval(i, mills);

                }
                
            }
               
        }
        public void Status()
        {
            try
            {
                if (Replicas != null)
                {

                    for (int i = 0; i < Replicas.Count; i++) {
                        if (Replicas[i] != null)
                        {
                            Replicas[i].Status(i);
                        }
                        else {
                            Console.WriteLine($"Replica from operator {ID} is null");
                        }
                    }


                        
                }
            } catch (NeighbourOperatorIsDeadException noide)
            {
                Console.WriteLine("Caught NeighbourOperatorIsDeadException");
            }
        }
        #endregion
    }
}
