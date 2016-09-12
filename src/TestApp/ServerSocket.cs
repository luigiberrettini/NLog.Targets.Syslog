using System;
using System.Net;
using System.Net.Sockets;

namespace TestApp
{
    internal abstract class ServerSocket : IDisposable
    {
        protected volatile bool KeepGoing;

        protected Socket BoundSocket { get; private set; }

        protected ProtocolType ProtocolType { get; set; }

        protected SocketType SocketType { get; set; }

        protected Action<string> OnReceivedString { get; private set; }

        public void StartListening(IPEndPoint ipEndPoint, Action<string> receivedStringAction)
        {
            if (KeepGoing)
                return;
            KeepGoing = true;

            OnReceivedString = receivedStringAction;

            BoundSocket = new Socket(AddressFamily.InterNetwork, SocketType, ProtocolType);
            SetupSocket(ipEndPoint);

            while (KeepGoing)
                Receive();
        }

        protected virtual void SetupSocket(IPEndPoint ipEndPoint)
        {
            BoundSocket.Bind(ipEndPoint);
        }

        protected abstract void Receive();

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            KeepGoing = false;
            BoundSocket.Dispose();
        }
    }
}