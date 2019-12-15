// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using System.Text;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class Rfc3164 : MessageBuilder
    {
        private const string TimestampFormat = "{0:MMM} {0,11:d HH:mm:ss}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        private readonly bool outputPri;
        private readonly bool outputHeader;
        private readonly Layout hostnameLayout;
        private readonly Layout tagLayout;
        private readonly PlainHostnamePolicySet hostnamePolicySet;
        private readonly TagPolicySet tagPolicySet;
        private readonly PlainContentPolicySet plainContentPolicySet;
        private readonly AsciiMessagePolicy asciiMessagePolicy;

        public Rfc3164(Facility facility, LogLevelSeverityConfig logLevelSeverityConfig, Rfc3164Config rfc3164Config, EnforcementConfig enforcementConfig) : base(facility, logLevelSeverityConfig, enforcementConfig)
        {
            hostnamePolicySet = new PlainHostnamePolicySet(enforcementConfig);
            tagPolicySet = new TagPolicySet(enforcementConfig);
            plainContentPolicySet = new PlainContentPolicySet(enforcementConfig);
            asciiMessagePolicy = new AsciiMessagePolicy(enforcementConfig);

            outputPri = rfc3164Config.OutputPri;
            outputHeader = rfc3164Config.OutputHeader;
            hostnameLayout = rfc3164Config.Hostname;
            tagLayout = rfc3164Config.Tag;
        }

        protected override void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string pri, string logEntry)
        {
            var encoding = new ASCIIEncoding();
            AppendPriBytes(buffer, pri, encoding);
            AppendHeaderBytes(buffer, logEvent, encoding);
            buffer.Append(SpaceBytes);
            AppendMsgBytes(buffer, logEvent, logEntry, encoding);

            asciiMessagePolicy.Apply(buffer);
        }

        private void AppendPriBytes(ByteArray buffer, string pri, Encoding encoding)
        {
            if (!outputPri)
                return;
            buffer.Append(pri, encoding);
        }

        private void AppendHeaderBytes(ByteArray buffer, LogEventInfo logEvent, Encoding encoding)
        {
            if (!outputHeader)
                return;
            var timestamp = logEvent.TimeStamp.ToString(TimestampFormat, CultureInfo.InvariantCulture);
            var host = hostnamePolicySet.Apply(hostnameLayout.Render(logEvent));
            buffer.Append(timestamp, encoding);
            buffer.Append(SpaceBytes);
            buffer.Append(host, encoding);
        }

        private void AppendMsgBytes(ByteArray buffer, LogEventInfo logEvent, string logEntry, Encoding encoding)
        {
            AppendTagBytes(buffer, logEvent, encoding);
            AppendContentBytes(buffer, logEntry, encoding);
        }

        private void AppendTagBytes(ByteArray buffer, LogEventInfo logEvent, Encoding encoding)
        {
            var tag = tagPolicySet.Apply(tagLayout.Render(logEvent));
            buffer.Append(tag, encoding);
        }

        private void AppendContentBytes(ByteArray buffer, string logEntry, Encoding encoding)
        {
            var content = plainContentPolicySet.Apply(logEntry);
            buffer.Append(content, encoding);
        }
    }
}