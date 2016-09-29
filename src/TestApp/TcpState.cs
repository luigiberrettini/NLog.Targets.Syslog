using System;
using System.IO;
using System.Net.Sockets;
using NLog.Targets.Syslog.Settings;

namespace TestApp
{
    internal class TcpState : StateObject
    {
        private volatile bool firstReceive;
        private FramingMethod? framing;

        public TcpState(Socket receiveSocket) : base(receiveSocket)
        {
            firstReceive = true;
            framing = null;
        }

        protected override void HandleFirstReceive(MemoryStream ms)
        {
            if (!firstReceive)
                return;

            firstReceive = false;
            DetectFraming(ms);
        }

        protected override void HandleChunks(string receivedString, Action<string> receivedStringAction)
        {
            var msgSet = ServerMsgSet.FromStringAndFraming(receivedString, framing);

            if (msgSet == null)
            {
                receivedStringAction($"Error handling string {receivedString}");
                return;
            }

            for (var i = 0; i < msgSet.FullMessages; i++)
            {
                var msg = msgSet[i];
                receivedStringAction(msgSet.IsValid(msg, framing) ? msg : $"Error handling string {receivedString}");
            }

            if (!msgSet.LastIsPartial)
                return;

            Buffer.SetLength(0);
            var lastMessage = msgSet.LastMessageBytes;
            Buffer.Write(lastMessage, 0, lastMessage.Length);
        }

        private void DetectFraming(MemoryStream ms)
        {
            var firstByte = ms.GetBuffer()[0];
            var firstChar = Convert.ToChar(firstByte);

            if (char.IsDigit(firstChar))
                framing = FramingMethod.OctetCounting;
            else if (firstChar == '<')
                framing = FramingMethod.NonTransparent;
        }
    }
}