using ProcessCreationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    class PCServer
    {
        private TcpListener listener = null;
        private List<PCSocketListener> socketListeners = null;
        private Thread serverThread;
        private bool done = false;
        public readonly int port = 10000;
        public PCServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
            }
            catch (Exception e)
            {
                listener = null;
            }
        }

        public void StartServer()
        {
            if (listener != null)
            {
                socketListeners = new List<PCSocketListener>();

                // Start the Server
                listener.Start();
                serverThread = new Thread(new ThreadStart(ServerThreadStart));
                serverThread.Start();
            }
        }

        private void ServerThreadStart()
        {
            // Client Socket variable;
            Socket clientSocket = null;
            PCSocketListener socketListener = null;
           
            while (!done)
            {
                try
                {
                    // Wait for any client requests and if there is any 
                    // request from any client accept it (Wait indefinitely).
                    clientSocket = listener.AcceptSocket();

                    // Create a SocketListener object for the client.
                    socketListener = new PCSocketListener(clientSocket);
                    socketListener.StartSocketListener();
                }
                catch (SocketException se)
                {
                    done = true;
                }
            }
        }

        public void StopServer()
        {
            if (listener != null)
            {
                // Stop the TCP/IP Server.
                done = true;
                listener.Stop();

                // Wait for one second for the the thread to stop.
                serverThread?.Join(1000);

                // If still alive; Get rid of the thread.
                if (serverThread.IsAlive)
                {
                    serverThread.Abort();
                }
                serverThread = null;
                listener = null;
            }
        }
    }
}
