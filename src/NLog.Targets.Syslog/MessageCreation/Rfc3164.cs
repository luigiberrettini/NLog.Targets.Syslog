using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>Allows to build Syslog messages compliant with RFC 3164</summary>
    [NLogConfigurationItem]
    public class Rfc3164 : MessageBuilder
    {
        private PlainHostnamePolicySet hostnamePolicySet;
        private TagPolicySet tagPolicySet;
        private PlainContentPolicySet plainContentPolicySet;
        private AsciiMessagePolicy asciiMessagePolicy;
        private const string TimestampFormat = "{0:MMM} {0,11:d HH:mm:ss}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname { get; set; }

        /// <summary>The TAG field of the MSG part</summary>
        public Layout Tag { get; set; }

        /// <summary>Builds a new instance of the Rfc3164 class</summary>
        public Rfc3164()
        {
            Hostname = Dns.GetHostName();
            Tag = Assembly.GetCallingAssembly().GetName().Name;
        }

        internal override void Initialize(Enforcement enforcement)
        {
            base.Initialize(enforcement);
            hostnamePolicySet = new PlainHostnamePolicySet(enforcement);
            tagPolicySet = new TagPolicySet(enforcement);
            plainContentPolicySet = new PlainContentPolicySet(enforcement);
            asciiMessagePolicy = new AsciiMessagePolicy(enforcement);
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
            var hostname = hostnamePolicySet.Apply(Hostname.Render(logEvent));
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
            var tag = tagPolicySet.Apply(Tag.Render(logEvent));
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