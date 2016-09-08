using System;
using System.Net;
using System.Net.Sockets;

namespace TestApp
{
    internal abstract class ServerSocket : IDisposable
    {
        private volatile bool continueListening;

        protected ProtocolType ProtocolType { get; set; }

        protected SocketType SocketType { get; set; }

        protected Action<string> OnReceivedString { get; private set; }

        public void StartListening(IPEndPoint ipEndPoint, Action<string> receivedStringAction)
        {
            if (continueListening)
                return;
            continueListening = true;

            OnReceivedString = receivedStringAction;

            var boundSocket = new Socket(AddressFamily.InterNetwork, SocketType, ProtocolType);
            SetupSocket(ipEndPoint, boundSocket);

            while (continueListening)
                Receive(boundSocket);
        }

        protected virtual void SetupSocket(IPEndPoint ipEndPoint, Socket boundSocket)
        {
            boundSocket.Bind(ipEndPoint);
        }

        protected abstract void Receive(Socket receivingSocket);

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                continueListening = false;
        }
    }
}