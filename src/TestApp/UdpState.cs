using System;
using System.IO;
using System.Net.Sockets;

namespace TestApp
{
    internal class UdpState : StateObject
    {
        public UdpState(Socket receiveSocket) : base(receiveSocket)
        {
        }

        protected override void HandleFirstReceive(MemoryStream ms)
        {
        }

        protected override void HandleChunks(string receivedString, Action<string> receivedStringAction)
        {
            receivedStringAction(receivedString);
            Buffer.SetLength(0);
        }
    }
}