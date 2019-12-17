// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FakeSyslogServer
{
    internal class ServerMsgSet
    {
        private string[] messages;

        private string LastMessage => messages[messages.Length - 1];

        public bool LastIsPartial { get; private set; }

        public int FullMessages => messages.Length - (LastIsPartial ? 1 : 0);

        public string this[int index] => messages[index];

        public byte[] LastMessageBytes => Encoding.UTF8.GetBytes(LastMessage);

        public static ServerMsgSet FromStringAndFraming(string s, int? framing)
        {
            var msgSet = new ServerMsgSet();
            if (framing == TcpState.NonTransparentHashCode)
                return FromStringNonTransparent(s, msgSet);
            if (framing == TcpState.OctetCountingHashCode)
                return FromStringOctetCounting(s, msgSet);
            return null;
        }

        public bool IsValid(string message, int? framing)
        {
            if (framing == null)
                return false;

            if (framing == TcpState.NonTransparentHashCode)
                return true;

            var splitted = Regex.Split(message, "(\\d{1,11}) (<\\d{3}>)").Where(x => x != string.Empty).ToArray();
            var octetCount = splitted[0];
            var octets = splitted.Length < 3 ? null : Encoding.UTF8.GetBytes(splitted[1] + splitted[2]);
            return octetCount == octets?.Length.ToString();
        }

        private static ServerMsgSet FromStringNonTransparent(string s, ServerMsgSet serverMsgSet)
        {
            const char lineFeed = '\n';
            serverMsgSet.messages = s.Split(lineFeed);

            var lastChar = s[s.Length - 1];
            serverMsgSet.LastIsPartial = lastChar == lineFeed;

            return serverMsgSet;
        }

        private static ServerMsgSet FromStringOctetCounting(string s, ServerMsgSet serverMsgSet)
        {
            var matches = Regex
                .Split(s, "(\\d{1,11} <\\d{1,3}>)")
                .Where(x => !string.IsNullOrEmpty(x))
                .Select((value, index) => new { value, index })
                .ToArray();
            serverMsgSet.messages = matches
                .Where(x => x.index % 2 == 0)
                .Select(x => x.value + (matches.Length <= x.index + 1 ? string.Empty : matches[x.index + 1].value))
                .ToArray();

            serverMsgSet.LastIsPartial = !serverMsgSet.IsValid(serverMsgSet.LastMessage, TcpState.OctetCountingHashCode);

            return serverMsgSet;
        }
    }
}