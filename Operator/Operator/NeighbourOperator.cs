using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    class NeighbourOperator : Destination
    {
        public List<IReplica> replicas;
        public RoutingStrategy RoutingStrategy { get; set; }
        public bool controlFlag = false;

        public NeighbourOperator(Replica master, DestinationInfo info, Semantic semantic) : base(master, semantic)
        {
            var replicasTask = Helper.GetAllStubs<IReplica>(info.Addresses);
            var initTask = Task.Run(async () =>
            {
                this.replicas = (await replicasTask).ToList();

                if (info.RtStrategy == SharedTypes.RoutingStrategy.Primary)
                {
                    RoutingStrategy = new PrimaryStrategy(replicas);
                }
                else if (info.RtStrategy == SharedTypes.RoutingStrategy.Hashing)
                {
                    RoutingStrategy = new HashingStrategy(replicas, info.HashingArg);
                }
                else
                {
                    RoutingStrategy = new RandomStrategy(replicas);
                }

            });
        }
        public override void Deliver(CTuple tuple)
        {
            // Console.WriteLine($"NeighbourOperator: Delivering Tuple {tuple.ToString()}.");
            var rep = RoutingStrategy.ChooseReplica(tuple);
            switch (Semantic)
            {
                case Semantic.AtLeastOnce:
                    controlFlag = false;
                    
                    while (!controlFlag)
                    {
                        try
                        {
                            rep.ProcessAndForward(tuple);
                            controlFlag = true;
                            Console.WriteLine("********Processed");
                        }
                        //FIXME Exceção que faz timeout automatico 
                        catch (Exception e) { Console.WriteLine("**********Exception"); };
                    }
                    //Console.WriteLine($"The semantic At-Least-Once hasn't been implemented yet. Please consider using at-most-once instead...");
                    break;
                case Semantic.AtMostOnce:
                    rep.ProcessAndForward(tuple);
                    break;
                case Semantic.ExactlyOnce:
                    //Problema: O custom escreve para ficheiros, se falha a meio volta a escrever
                    //Transações ?? custom 
                    //Na replica, antes de fazer process verificar -> ter um id por tuplo a ser processado e verifico se esse id já foi processado é só 
                    Console.WriteLine($"The semantic exaclty-Once hasn't been implemented yet. Please consider using at-most-once instead...");
                    break;
                default:
                    Console.WriteLine($"The specified semantic ({Semantic}) is not supported within our system");
                    return;
            }
        }
        public override void Ping()
        {
            // Just need to ensure that one replica is alive
            if (replicas != null && replicas.Count > 0)
                foreach (IReplica rep in replicas)
                {
                    try
                    {
                        var task = Task.Run(() => rep.Ping());
                        if (task.Wait(TimeSpan.FromMilliseconds(10)))
                            return;
                    }
                    catch (Exception e)
                    {
                        // does nothing, there might be a working replica
                    }
                }
            // there are no more replicas 
            throw new NeighbourOperatorIsDeadException("Neighbour Operator has no working replicas.");
        }

    }
}
