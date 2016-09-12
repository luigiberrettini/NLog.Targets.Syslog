using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TestApp
{
    internal class TcpState : StateObject
    {
        private volatile bool firstReceive;
        private volatile bool octetCounting;
        private int octetsToReceive;

        public TcpState(Socket receiveSocket) : base(receiveSocket)
        {
            firstReceive = true;
        }

        protected override void HandleFirstReceive(MemoryStream ms, string str)
        {
            if (!firstReceive)
                return;

            firstReceive = false;
            HandleFirstChunk(ms, str);
        }

        private void HandleFirstChunk(Stream ms, string str)
        {
            octetCounting = char.IsDigit(str[0]);

            if (!IsFirstChunk(str))
            {
                ms.SetLength(0);
                return;
            }

            if (!octetCounting)
                return;

            var charsBeforePri = str.IndexOf(" <", StringComparison.Ordinal);
            var octectCount = str.Substring(0, charsBeforePri);
            octetsToReceive = charsBeforePri + 1 + int.Parse(octectCount);
        }

        private bool IsFirstChunk(string str)
        {
            if (!octetCounting)
            {
                var charsBeforePri = str.IndexOf("<", StringComparison.Ordinal);
                return charsBeforePri == 0;
            }

            var maxOctetCounLength = int.MaxValue.ToString().Length + 1;
            var charsBeforeSpacePri = str.IndexOf(" <", StringComparison.Ordinal);
            return charsBeforeSpacePri > -1 && charsBeforeSpacePri < maxOctetCounLength;
        }

        protected override void HandleLastReceive(StringBuilder receivedData, string str, Action<string> receivedStringAction)
        {
            if (octetCounting)
                HandleOctetCountingLastReceive(receivedData, str, receivedStringAction);
            else
                HandleNonTransparentLastReceive(receivedData, str, receivedStringAction);
        }

        private void HandleOctetCountingLastReceive(StringBuilder receivedData, string str, Action<string> receivedStringAction)
        {
            if (receivedData.Length < octetsToReceive)
                return;

            if (receivedData.Length == octetsToReceive)
            {
                receivedStringAction(receivedData.ToString());
                Buffer.SetLength(0);
                receivedData.Clear();
                firstReceive = true;
                return;
            }

            receivedStringAction(receivedData.ToString(0, octetsToReceive));
            var nextString = receivedData.ToString(octetsToReceive, receivedData.Length - octetsToReceive);
            receivedData.Clear();
            receivedData.Append(nextString);
            HandleFirstChunk(Buffer, nextString);
            HandleLastReceive(receivedData, nextString, receivedStringAction);
            firstReceive = true;
        }

        private void HandleNonTransparentLastReceive(StringBuilder receivedData, string str, Action<string> receivedStringAction)
        {
            var indexOfLineFeed = str.IndexOf("\n", StringComparison.Ordinal);

            if (indexOfLineFeed == -1)
                return;

            if (indexOfLineFeed == str.Length - 1)
            {
                receivedStringAction(receivedData.ToString());
                Buffer.SetLength(0);
                receivedData.Clear();
                firstReceive = true;
                return;
            }

            var newPosition = indexOfLineFeed + 1;
            receivedStringAction(receivedData.ToString(0, newPosition));
            var nextString = receivedData.ToString(newPosition, receivedData.Length - newPosition);
            receivedData.Clear();
            receivedData.Append(nextString);
            HandleFirstChunk(Buffer, nextString);
            HandleLastReceive(receivedData, nextString, receivedStringAction);
            firstReceive = true;
        }
    }
}