using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    class PCSocketListener
    {
        private Socket clientSocket;
        private Thread clientListenerThread;
        public PCSocketListener(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
        }
        
        public void StartSocketListener()
        {
            if (clientSocket != null)
            {
                clientListenerThread =
                  new Thread(new ThreadStart(SocketListenerThreadStart));

                clientListenerThread.Start();
            }
        }

        private void SocketListenerThreadStart()
        {
            int size = 0;
            Byte[] byteBuffer = new Byte[2048];
            string result = "";
            bool done = false;
            while (!done)
            {
                try
                {
                    size = clientSocket.Receive(byteBuffer);
                    done = true;
                    byteBuffer[size] = (byte)'\0';
                    result += System.Text.Encoding.ASCII.GetString(byteBuffer);
                }
                catch (SocketException se)
                {
                    done = true;
                }
            }
            Console.WriteLine("New Request:\n" + result);
            clientSocket.Close();
            CreateProcess(result);
        }

        private void CreateProcess(string arg)
        {
            Process.Start(ProcessCreationService.processName ?? "Operator.exe", arg);
        }
    }
}
