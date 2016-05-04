using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private EncodedContentPolicy encodedContentPolicy;
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

        /// <summary>Initializes the Rfc3164</summary>
        /// <param name="enforcement">The enforcement to apply</param>
        internal override void Initialize(Enforcement enforcement)
        {
            base.Initialize(enforcement);
            hostnamePolicySet = new PlainHostnamePolicySet(enforcement);
            tagPolicySet = new TagPolicySet(enforcement);
            plainContentPolicySet = new PlainContentPolicySet(enforcement);
            encodedContentPolicy = new EncodedContentPolicy(enforcement);
        }

        /// <summary>Builds the Syslog message according to RFC 3164</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Bytes containing the Syslog message</returns>
        protected override IEnumerable<byte> BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var encoding = new ASCIIEncoding();
            var msgPrefixBytes = PriBytes(pri, encoding)
                .Concat(HeaderBytes(logEvent, encoding))
                .Concat(SpaceBytes)
                .ToArray();
            var msgBytes = MsgBytes(logEvent, logEntry, msgPrefixBytes.Length, encoding);
            return msgPrefixBytes.Concat(msgBytes);
        }

        private static IEnumerable<byte> PriBytes(string pri, Encoding encoding)
        {
            return encoding.GetBytes(pri);
        }

        private IEnumerable<byte> HeaderBytes(LogEventInfo logEvent, Encoding encoding)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = hostnamePolicySet.Apply(Hostname.Render(logEvent));
            var header = $"{timestamp} {hostname}";
            return encoding.GetBytes(header);
        }

        private IEnumerable<byte> MsgBytes(LogEventInfo logEvent, string logEntry, int msgPrefixSize, Encoding encoding)
        {
            var tag = tagPolicySet.Apply(Tag.Render(logEvent));
            var tagBytes = encoding.GetBytes(tag);
            var contentPrefixLength = msgPrefixSize + tag.Length;
            var contentBytes = ContentBytes(logEntry, contentPrefixLength, encoding);
            return tagBytes.Concat(contentBytes);
        }

        private IEnumerable<byte> ContentBytes(string logEntry, int contentPrefixLength, Encoding encoding)
        {
            var plainContent = plainContentPolicySet.Apply(logEntry);
            var encodedContent = encoding.GetBytes(plainContent);
            return encodedContentPolicy.Apply(encodedContent, contentPrefixLength);
        }
    }
}