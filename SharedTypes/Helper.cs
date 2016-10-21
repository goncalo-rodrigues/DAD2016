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

            /*Replica obj = (Replica)Activator.GetObject(
                typeof(Replica), );*/
            throw new NotImplementedException();
        }
    }
}
