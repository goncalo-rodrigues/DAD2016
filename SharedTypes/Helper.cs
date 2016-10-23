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

            T obj = (T)Activator.GetObject(typeof(T), address);

            return obj;
        }
    }
}
