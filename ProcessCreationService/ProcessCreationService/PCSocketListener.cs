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

                Console.WriteLine("Accepting request at " + clientSocket.RemoteEndPoint.ToString());
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
                    if (size == 0 || byteBuffer[size-1] == (byte) '\0')
                        done = true;
                    byteBuffer[size] = (byte)'\0';
                    result += System.Text.Encoding.ASCII.GetString(byteBuffer);
                }
                catch (SocketException se)
                {
                    done = true;
                }
            }
            try
            {
                var process = CreateProcess(result);
                var response = process == null ? -1 : process.Id;
                clientSocket.Send(BitConverter.GetBytes(response));
                clientSocket.Close();
            } catch (SocketException se)
            {

            }

            
        }

        private Process CreateProcess(string arg)
        {
            try
            {
                var processName = "C:\\Users\\Goncalo\\Source\\Repos\\DAD2016\\Operator\\Operator\\bin\\Debug\\Operator.exe";
                var pro = Process.Start(processName, $"\"{arg.Replace("\"", "\\\"")}\"");
                return pro;
            } catch (Exception) { };
            return null;
           
        }
    }
}
