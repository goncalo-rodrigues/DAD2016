using SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Operator
{
    public abstract class BufferedOperator
    {
        const int BUFFER_SIZE = 1024;
        public int BufferSize { get; }
        public BlockingCollection<CTuple> Buffer { get; set; }
        public CTuple LastTakenTuple = null;
        public BufferedOperator(bool takeTupleAsSoonAsItArrives = true) : this(BUFFER_SIZE, takeTupleAsSoonAsItArrives) { }
        public BufferedOperator(int BufferSize, bool takeTupleAsSoonAsItArrives = true)
        {
            this.BufferSize = BufferSize;
            this.Buffer = new BlockingCollection<CTuple>(new ConcurrentQueue<CTuple>(), BufferSize);
            if (takeTupleAsSoonAsItArrives)
                Task.Run(() => Processor());
        }
        public virtual CTuple Take()
        {
            return LastTakenTuple = Buffer.Take();
        }
        public int Count()
        {
            return Buffer.Count;
        }      
        public void Insert(CTuple tuple)
        {
            try
            {
                Buffer?.Add(tuple);
            }
            catch (Exception e)
            {
                Console.WriteLine($"//DEBUG: The tuple was not inserted due to {e.GetBaseException()}//");
            }
        }
        public void MarkFinish() // marks the buffer has not accepting further tuples
        {
            Buffer?.CompleteAdding();
        }
        private void Processor()
        {
            while (!Buffer.IsCompleted)
            {
                // Blocks if number.Count == 0
                // IOE means that Take() was called on a completed collection.
                // Some other thread can call CompleteAdding after we pass the
                // IsCompleted check but before we call Take. 
                // In this example, we can simply catch the exception since the 
                // loop will break on the next iteration.
                try
                {
                    LastTakenTuple = Buffer.Take();
                }
                catch (InvalidOperationException) { }

                if (LastTakenTuple != null)
                {
                    DoStuff(LastTakenTuple);
                }
            }
        }
        public string Status()
        {
            return $"lastTaken: {LastTakenTuple?.ID}, count: {Buffer.Count}";
        }
        public abstract void DoStuff(CTuple tuple);
    }
}
