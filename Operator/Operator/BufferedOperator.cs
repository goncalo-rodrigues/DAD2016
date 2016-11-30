using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Operator
{
    public abstract class BufferedOperator
    {
        const int BUFFER_SIZE = 128;
        public int BufferSize { get; }
        private BlockingCollection<CTuple> buffer;
  
        public BufferedOperator() : this(BUFFER_SIZE) { }
        public BufferedOperator(int BufferSize)
        {
            this.BufferSize = BufferSize;
            this.buffer = new BlockingCollection<CTuple>(new ConcurrentQueue<CTuple>(), BufferSize);

            Task.Run(() => Processor());
        }
        
        private void Processor()
        {
            while (!buffer.IsCompleted)
            {
                CTuple tuple = null;
                // Blocks if number.Count == 0
                // IOE means that Take() was called on a completed collection.
                // Some other thread can call CompleteAdding after we pass the
                // IsCompleted check but before we call Take. 
                // In this example, we can simply catch the exception since the 
                // loop will break on the next iteration.
                try
                {
                    tuple = buffer.Take();
                }
                catch (InvalidOperationException) { }

                if (tuple != null)
                {
                    DoStuff(tuple);
                }
            }
        }


        public void Insert(CTuple tuple)
        {
            try
            {
                buffer?.Add(tuple);
            }
            catch (Exception e)
            {
                Console.WriteLine($"//DEBUG: The tuple was not inserted due to {e.GetBaseException()}//");
            }
        }
        public void MarkFinish() // marks the buffer has not accepting further tuples
        {
            buffer?.CompleteAdding();
        }
        public abstract void DoStuff(CTuple tuple);

    }
}
