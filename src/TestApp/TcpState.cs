using System;
using System.Net.Sockets;

namespace TestApp
{
    internal class TcpState : StateObject
    {
        private volatile bool isFirstReceive;
        private volatile bool octetCounting;
        private int octetsToReceive;

        public TcpState(Socket receiveSocket) : base(receiveSocket)
        {
            isFirstReceive = true;
        }

        public override void BeginReceive(AsyncCallback readCallback)
        {
            ReceiveSocket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, readCallback, this);
        }

        protected override void HandleFirstReceive(string str)
        {
            if (!isFirstReceive)
                return;

            isFirstReceive = false;
            octetCounting = char.IsDigit(str[0]);
            if (!octetCounting)
                return;

            var charsBeforePri = str.IndexOf(" <", StringComparison.Ordinal);
            var octectCount = str.Substring(0, charsBeforePri);
            octetsToReceive = charsBeforePri + 1 + int.Parse(octectCount);
        }

        protected override bool IsLastReceive(string str)
        {
            return (octetCounting && ReceivedData.Length == octetsToReceive) ||
                (!octetCounting && str.IndexOf("\n", StringComparison.Ordinal) != -1);
        }

        protected override int EndReceive(IAsyncResult asyncResult)
        {
            return ReceiveSocket.EndReceive(asyncResult);
        }
    }
}