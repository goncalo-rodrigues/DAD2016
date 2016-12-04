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
  
        public BufferedOperator(bool takeTupleAsSoonAsItArrives = true) : this(BUFFER_SIZE, takeTupleAsSoonAsItArrives) { }
        public BufferedOperator(int BufferSize, bool takeTupleAsSoonAsItArrives = true)
        {
            this.BufferSize = BufferSize;
            this.buffer = new BlockingCollection<CTuple>(new ConcurrentQueue<CTuple>(), BufferSize);
            if (takeTupleAsSoonAsItArrives)
                Task.Run(() => Processor());
        }
        public virtual CTuple Take()
        {
            return buffer.Take();
        }
        public int Count()
        {
            return buffer.Count;
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
        public abstract void DoStuff(CTuple tuple);
    }
}
