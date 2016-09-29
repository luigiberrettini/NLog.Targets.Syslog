using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class Rfc5424 : MessageBuilder
    {
        private const string DefaultVersion = "1";
        private const string NilValue = "-";
        private const string TimestampFormat = "{0:yyyy-MM-ddTHH:mm:ss.ffffffK}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        private readonly string version;
        private readonly Layout hostname;
        private readonly Layout appName;
        private readonly Layout procId;
        private readonly Layout msgId;
        private readonly StructuredData structuredData;
        private readonly bool disableBom;
        private readonly FqdnHostnamePolicySet hostnamePolicySet;
        private readonly AppNamePolicySet appNamePolicySet;
        private readonly ProcIdPolicySet procIdPolicySet;
        private readonly MsgIdPolicySet msgIdPolicySet;
        private readonly Utf8MessagePolicy utf8MessagePolicy;

        public Rfc5424(Facility facility, Rfc5424Config rfc5424Config, EnforcementConfig enforcementConfig) : base(facility, enforcementConfig)
        {
            version = DefaultVersion;
            hostname = rfc5424Config.DefaultHostname;
            appName = rfc5424Config.DefaultAppName;
            procId = NilValue;
            msgId = NilValue;
            structuredData = new StructuredData(rfc5424Config.StructuredData, enforcementConfig);
            disableBom = false;
            hostnamePolicySet = new FqdnHostnamePolicySet(enforcementConfig, rfc5424Config.DefaultHostname);
            appNamePolicySet = new AppNamePolicySet(enforcementConfig, rfc5424Config.DefaultAppName);
            procIdPolicySet = new ProcIdPolicySet(enforcementConfig);
            msgIdPolicySet = new MsgIdPolicySet(enforcementConfig);
            utf8MessagePolicy = new Utf8MessagePolicy(enforcementConfig);
        }

        protected override ByteArray BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var encodings = new EncodingSet(!disableBom);

            AppendHeaderBytes(pri, logEvent, encodings);
            Message.Append(SpaceBytes);
            AppendStructuredDataBytes(logEvent, encodings);
            Message.Append(SpaceBytes);
            AppendMsgBytes(logEntry, encodings);

            utf8MessagePolicy.Apply(Message);

            return Message;
        }

        private void AppendHeaderBytes(string pri, LogEventInfo logEvent, EncodingSet encodings)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = hostnamePolicySet.Apply(this.hostname.Render(logEvent));
            var appName = appNamePolicySet.Apply(this.appName.Render(logEvent));
            var procId = procIdPolicySet.Apply(this.procId.Render(logEvent));
            var msgId = msgIdPolicySet.Apply(this.msgId.Render(logEvent));
            var header = $"{pri}{version} {timestamp} {hostname} {appName} {procId} {msgId}";
            var headerBytes = encodings.Ascii.GetBytes(header);
            Message.Append(headerBytes);
        }

        private void AppendStructuredDataBytes(LogEventInfo logEvent, EncodingSet encodings)
        {
            structuredData.AppendBytes(Message, logEvent, encodings);
        }

        private void AppendMsgBytes(string logEntry, EncodingSet encodings)
        {
            AppendPreambleBytes(encodings);
            AppendLogEntryBytes(logEntry, encodings);
        }

        private void AppendPreambleBytes(EncodingSet encodings)
        {
            var preambleBytes = encodings.Utf8.GetPreamble();
            Message.Append(preambleBytes);
        }

        private void AppendLogEntryBytes(string logEntry, EncodingSet encodings)
        {
            var logEntryBytes = encodings.Utf8.GetBytes(logEntry);
            Message.Append(logEntryBytes);
        }
    }
}