using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using System.Text;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>Allows to build Syslog messages compliant with RFC 3164</summary>
    internal class Rfc3164 : MessageBuilder
    {
        private const string TimestampFormat = "{0:MMM} {0,11:d HH:mm:ss}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        private readonly Layout hostname;
        private readonly Layout tag;
        private readonly PlainHostnamePolicySet hostnamePolicySet;
        private readonly TagPolicySet tagPolicySet;
        private readonly PlainContentPolicySet plainContentPolicySet;
        private readonly AsciiMessagePolicy asciiMessagePolicy;

        /// <summary>Builds a new instance of the Rfc3164 class</summary>
        public Rfc3164(Facility facility, Rfc3164Config rfc3164Config, EnforcementConfig enforcementConfig) : base(facility, enforcementConfig)
        {
            hostnamePolicySet = new PlainHostnamePolicySet(enforcementConfig);
            tagPolicySet = new TagPolicySet(enforcementConfig);
            plainContentPolicySet = new PlainContentPolicySet(enforcementConfig);
            asciiMessagePolicy = new AsciiMessagePolicy(enforcementConfig);

            hostname = rfc3164Config.Hostname;
            tag = rfc3164Config.Tag;
        }

        protected override ByteArray BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var encoding = new ASCIIEncoding();

            AppendPriBytes(pri, encoding);
            AppendHeaderBytes(logEvent, encoding);
            Message.Append(SpaceBytes);
            AppendMsgBytes(logEvent, logEntry, encoding);

            asciiMessagePolicy.Apply(Message);

            return Message;
        }

        private void AppendPriBytes(string pri, Encoding encoding)
        {
            var priBytes = encoding.GetBytes(pri);
            Message.Append(priBytes);
        }

        private void AppendHeaderBytes(LogEventInfo logEvent, Encoding encoding)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = hostnamePolicySet.Apply(this.hostname.Render(logEvent));
            var header = $"{timestamp} {hostname}";
            var headerBytes = encoding.GetBytes(header);
            Message.Append(headerBytes);
        }

        private void AppendMsgBytes(LogEventInfo logEvent, string logEntry, Encoding encoding)
        {
            AppendTagBytes(logEvent, encoding);
            AppendContentBytes(logEntry, encoding);
        }

        private void AppendTagBytes(LogEventInfo logEvent, Encoding encoding)
        {
            var tag = tagPolicySet.Apply(this.tag.Render(logEvent));
            var tagBytes = encoding.GetBytes(tag);
            Message.Append(tagBytes);
        }

        private void AppendContentBytes(string logEntry, Encoding encoding)
        {
            var plainContent = plainContentPolicySet.Apply(logEntry);
            var encodedContent = encoding.GetBytes(plainContent);
            Message.Append(encodedContent);
        }
    }
}