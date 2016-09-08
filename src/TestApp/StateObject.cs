using System;
using System.Net.Sockets;
using System.Text;

namespace TestApp
{
    internal abstract class StateObject
    {
        protected const int BufferSize = 65536;
        private readonly UTF8Encoding encoding;

        protected StringBuilder ReceivedData { get; }

        protected byte[] Buffer { get; }

        protected Socket ReceiveSocket { get; }

        protected StateObject(Socket socket)
        {
            encoding = new UTF8Encoding();
            ReceivedData = new StringBuilder();
            Buffer = new byte[BufferSize];
            ReceiveSocket = socket;
        }

        public abstract void BeginReceive(AsyncCallback readCallback);

        public void EndReceive(IAsyncResult asyncResult, AsyncCallback readCallback, Action<string> receivedStringAction)
        {
            var bytesRead = EndReceive(asyncResult);

            if (bytesRead <= 0)
                return;

            var str = encoding.GetString(Buffer, 0, bytesRead);
            ReceivedData.Append(str);

            HandleFirstReceive(str);

            if (IsLastReceive(str))
                HandleLastReceive(receivedStringAction);
            BeginReceive(readCallback);
        }

        protected abstract void HandleFirstReceive(string str);

        protected abstract bool IsLastReceive(string str);

        private void HandleLastReceive(Action<string> receivedStringAction)
        {
            var receivedString = ReceivedData.ToString();
            ReceivedData.Length = 0;
            receivedStringAction(receivedString);
        }

        protected abstract int EndReceive(IAsyncResult asyncResult);
    }
}