using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using SharedTypes;

namespace Operator
{
    public class LoggerDestination : Destination
    {
        private ILogger Logger { get; set; } = null;
        
        private string SenderID { get; set; } = null;

        public LoggerDestination(Replica parent, Semantic semantic, string senderID, string loggerUrl) : base(parent, semantic)
        {
            SenderID = senderID;
            Logger = (ILogger)Activator.GetObject(typeof(ILogger), loggerUrl);
            if (Logger == null)
                Console.WriteLine($"Could not locate logging service at {loggerUrl}");
            else
                Console.WriteLine("PMLogService was successfully initiated.");
        } 

        override public void Deliver(CTuple tuple)
        {
            // Console.WriteLine($"Logging deliver: {tuple.ToString()}");
            if (Logger != null)
                Logger.Notify(new Record("tuple", SenderID, tuple.ToString(), DateTime.Now));

            Console.WriteLine($"Delivering {tuple}");
        }

        override public void Ping()
        {
            // does nothing
            return;
        }
    }
}
