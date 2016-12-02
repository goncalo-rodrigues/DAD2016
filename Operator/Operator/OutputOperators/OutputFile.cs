using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;
using System.IO;

namespace Operator
{
    public class OutputFile : Destination, IDisposable
    {
        public string FilePath { get; set; }
        private TextWriter OutputStream { get; set; }
        public OutputFile(Replica parent, Semantic semantic, string filepath = "output.dat") : base(parent, semantic)
        {
            FilePath = filepath;
            OutputStream = new StreamWriter(filepath);
        }

        public override void Deliver(CTuple tuple)
        {
            Console.WriteLine($"Writing to file: {tuple}");
            OutputStream.WriteLine(tuple);
            OutputStream.Flush();
        }

        public override void Ping()
        {
            return;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                OutputStream.Flush();
                OutputStream.Close();
                disposedValue = true;
            }
        }

         ~OutputFile()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
