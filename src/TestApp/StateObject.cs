using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TestApp
{
    internal abstract class StateObject
    {
        private const int BufferSize = 65536;
        private readonly UTF8Encoding encoding;
        private readonly Socket receiveSocket;
        private readonly StringBuilder receivedData;

        protected MemoryStream Buffer { get; }

        protected StateObject(Socket socket)
        {
            encoding = new UTF8Encoding();
            receiveSocket = socket;
            Buffer = new MemoryStream(BufferSize);
            receivedData = new StringBuilder();
        }

        public void BeginReceive(AsyncCallback readCallback)
        {
            receiveSocket.BeginReceive(Buffer.GetBuffer(), 0, BufferSize, SocketFlags.None, readCallback, this);
        }

        public void EndReceive(IAsyncResult asyncResult, AsyncCallback readCallback, Action<string> receivedStringAction)
        {
            var bytesRead = EndReceive(asyncResult);

            if (bytesRead <= 0)
                return;

            var bufferToString = BufferToString(0, bytesRead);
            receivedData.Append(bufferToString);
            HandleFirstReceive(Buffer, bufferToString);
            HandleLastReceive(receivedData, bufferToString, receivedStringAction);
            BeginReceive(readCallback);
        }

        protected string BufferToString(int position, int bytesRead)
        {
            return encoding.GetString(Buffer.GetBuffer(), position, bytesRead);
        }

        protected abstract void HandleFirstReceive(MemoryStream ms, string str);

        protected abstract void HandleLastReceive(StringBuilder sb, string str, Action<string> receivedStringAction);

        private int EndReceive(IAsyncResult asyncResult)
        {
            return receiveSocket.EndReceive(asyncResult);
        }
    }
}