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
                    size = clientSocket.Receive(byteBuffer, byteBuffer.Length - 1, SocketFlags.None);
                    if (size == 0)
                        done = true;
                    byteBuffer[size] = (byte)'\0';
                    result += System.Text.Encoding.ASCII.GetString(byteBuffer);
                }
                catch (SocketException se)
                {
                    done = true;
                }
            }
            var process = CreateProcess(result);
            //clientSocket.Send(process?.Id ?? -1);
            clientSocket.Close();
            
        }

        private Process CreateProcess(string arg)
        {
            try
            {
                var processName = ProcessCreationService.processName ?? "Operator.exe";
                var pro = Process.Start(processName, arg);
                return pro;
            } catch (Exception) { };
            return null;
           
        }
    }
}
