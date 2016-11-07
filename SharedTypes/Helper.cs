using System;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

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

        public async static Task<T[]> GetAllStubs<T>(IList<string> addresses, int maxTries = 3)
        {
            Task<T>[] tasks = new Task<T>[addresses.Count];
            List<T> result = new List<T>();
            int current = 0;
            foreach (var address in addresses) {
                
                tasks[current]  = Task.Run(() =>
                {
                    if (address == null)
                        return default(T);
                    var done = false;
                    var tries = 0;
                    T obj = default(T);
                    while (!done &&  tries < maxTries)
                    {
                        Thread.Sleep(100*tries);
                        try
                        {
                            obj = GetStub<T>(address);
                            done = true;
                        } catch (Exception)
                        {
                            
                        }
                        tries++;
                    }
                    if (done)
                    {
                        Console.WriteLine($"Established communication to {address} after {tries} tries.");
                    } else
                    {
                        Console.WriteLine($"Given up communication to {address} after {tries} tries.");
                    }
                    return obj;
                });
                current++;
            }
            var results = await Task.WhenAll(tasks);
            return results;
        }
    }
}
