using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog.Targets.Syslog.MessageSend;

namespace TestApp
{
    internal class MsgSet
    {
        private readonly UTF8Encoding encoding;
        private string[] messages;

        private string LastMessage => messages[messages.Length - 1];

        public bool LastIsPartial { get; private set; }

        public int FullMessages => messages.Length - (LastIsPartial ? 1 : 0);

        public string this[int index] => messages[index];

        public byte[] LastMessageBytes => encoding.GetBytes(LastMessage);

        public static MsgSet FromStringAndFraming(string s, FramingMethod? framing)
        {
            var msgSet = new MsgSet();
            switch (framing)
            {
                case FramingMethod.NonTransparent:
                {
                    return FromStringNonTransparent(s, msgSet);
                }
                case FramingMethod.OctetCounting:
                {
                    return FromStringOctetCounting(s, msgSet);
                }
                default:
                {
                    return null;
                }
            }
        }

        public bool IsValid(string message, FramingMethod? framing)
        {
            if (framing == null)
                return false;

            if (framing == FramingMethod.NonTransparent)
                return true;

            var splitted = Regex.Split(message, "(\\d{1,11}) (.*)").Where(x => x != string.Empty).ToArray();
            var octetCount = splitted[0];
            var octets = encoding.GetBytes(splitted[1]);
            return octetCount == octets.Length.ToString();
        }

        private static MsgSet FromStringNonTransparent(string s, MsgSet msgSet)
        {
            const char lineFeed = '\n';
            msgSet.messages = s.Split(lineFeed);

            var lastChar = s[s.Length - 1];
            msgSet.LastIsPartial = lastChar == lineFeed;

            return msgSet;
        }

        private static MsgSet FromStringOctetCounting(string s, MsgSet msgSet)
        {
            var matches = Regex
                .Split(s, "(\\d{1,11} <\\d{1,3}>)")
                .Where(x => !string.IsNullOrEmpty(x))
                .Select((value, index) => new { value, index})
                .ToArray();
            msgSet.messages = matches
                .Where(x => x.index % 2 == 0)
                .Select(x => x.value + matches[x.index + 1].value)
                .ToArray();

            msgSet.LastIsPartial = !msgSet.IsValid(msgSet.LastMessage, FramingMethod.OctetCounting);

            return msgSet;
        }

        private MsgSet()
        {
            encoding = new UTF8Encoding();
        }
    }
}