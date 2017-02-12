// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class Rfc5424 : MessageBuilder
    {
        private const string TimestampFormat = "{0:yyyy-MM-ddTHH:mm:ss.ffffffK}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        private readonly string version;
        private readonly Layout hostnameLayout;
        private readonly Layout appNameLayout;
        private readonly Layout procIdLayout;
        private readonly Layout msgIdLayout;
        private readonly StructuredData structuredData;
        private readonly bool disableBom;
        private readonly FqdnHostnamePolicySet hostnamePolicySet;
        private readonly AppNamePolicySet appNamePolicySet;
        private readonly ProcIdPolicySet procIdPolicySet;
        private readonly MsgIdPolicySet msgIdPolicySet;
        private readonly Utf8MessagePolicy utf8MessagePolicy;

        public Rfc5424(Facility facility, Rfc5424Config rfc5424Config, EnforcementConfig enforcementConfig) : base(facility, enforcementConfig)
        {
            version = rfc5424Config.Version;
            hostnameLayout = rfc5424Config.Hostname;
            appNameLayout = rfc5424Config.AppName;
            procIdLayout = rfc5424Config.ProcId;
            msgIdLayout = rfc5424Config.MsgId;
            structuredData = new StructuredData(rfc5424Config.StructuredData, enforcementConfig);
            disableBom = rfc5424Config.DisableBom;
            hostnamePolicySet = new FqdnHostnamePolicySet(enforcementConfig, rfc5424Config.DefaultHostname);
            appNamePolicySet = new AppNamePolicySet(enforcementConfig, rfc5424Config.DefaultAppName);
            procIdPolicySet = new ProcIdPolicySet(enforcementConfig);
            msgIdPolicySet = new MsgIdPolicySet(enforcementConfig);
            utf8MessagePolicy = new Utf8MessagePolicy(enforcementConfig);
        }

        protected override void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string pri, string logEntry)
        {
            var encodings = new EncodingSet(!disableBom);

            AppendHeaderBytes(buffer, pri, logEvent, encodings);
            buffer.Append(SpaceBytes);
            AppendStructuredDataBytes(buffer, logEvent, encodings);
            buffer.Append(SpaceBytes);
            AppendMsgBytes(buffer, logEntry, encodings);

            utf8MessagePolicy.Apply(buffer);
        }

        private void AppendHeaderBytes(ByteArray buffer, string pri, LogEventInfo logEvent, EncodingSet encodings)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = hostnamePolicySet.Apply(hostnameLayout.Render(logEvent));
            var appName = appNamePolicySet.Apply(appNameLayout.Render(logEvent));
            var procId = procIdPolicySet.Apply(procIdLayout.Render(logEvent));
            var msgId = msgIdPolicySet.Apply(msgIdLayout.Render(logEvent));
            var header = $"{pri}{version} {timestamp} {hostname} {appName} {procId} {msgId}";
            var headerBytes = encodings.Ascii.GetBytes(header);
            buffer.Append(headerBytes);
        }

        private void AppendStructuredDataBytes(ByteArray buffer, LogEventInfo logEvent, EncodingSet encodings)
        {
            structuredData.AppendBytes(buffer, logEvent, encodings);
        }

        private static void AppendMsgBytes(ByteArray buffer, string logEntry, EncodingSet encodings)
        {
            AppendPreambleBytes(buffer, encodings);
            AppendLogEntryBytes(buffer, logEntry, encodings);
        }

        private static void AppendPreambleBytes(ByteArray buffer, EncodingSet encodings)
        {
            var preambleBytes = encodings.Utf8.GetPreamble();
            buffer.Append(preambleBytes);
        }

        private static void AppendLogEntryBytes(ByteArray buffer, string logEntry, EncodingSet encodings)
        {
            var logEntryBytes = encodings.Utf8.GetBytes(logEntry);
            buffer.Append(logEntryBytes);
        }
    }
}