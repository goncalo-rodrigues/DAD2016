using System;

namespace SharedTypes
{
   public interface ILogger
    {
       void Notify(Record record);
        void ReRoute(string opName, int replicaId, string newAddr);
    }
}
