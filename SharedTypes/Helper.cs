using System;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace SharedTypes
{
    public static class Helper
    {
        // Should get a remote object given its address and its type
        public static T GetStub<T>(string address)
        {

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

             IReplica obj = (IReplica)Activator.GetObject(typeof(IReplica), address);

            return (T)obj;
        }
    }
}
