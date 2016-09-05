using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>Allows to build Syslog messages compliant with RFC 5424</summary>
    [NLogConfigurationItem]
    public class Rfc5424 : MessageBuilder
    {
        private readonly string defaultHostname;
        private readonly string defaultAppName;
        private const string DefaultVersion = "1";
        private const string NilValue = "-";
        private FqdnHostnamePolicySet hostnamePolicySet;
        private AppNamePolicySet appNamePolicySet;
        private ProcIdPolicySet procIdPolicySet;
        private MsgIdPolicySet msgIdPolicySet;
        private Utf8MessagePolicy utf8MessagePolicy;
        private const string TimestampFormat = "{0:yyyy-MM-ddTHH:mm:ss.ffffffK}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        /// <summary>The VERSION field of the HEADER part</summary>
        public string Version { get; }

        /// <summary>The HOSTNAME field of the HEADER part</summary>
        public Layout Hostname { get; set; }

        /// <summary>The APPNAME field of the HEADER part</summary>
        public Layout AppName { get; set; }

        /// <summary>The PROCID field of the HEADER part</summary>
        public Layout ProcId { get; set; }

        /// <summary>The MSGID field of the HEADER part</summary>
        public Layout MsgId { get; set; }

        /// <summary>The STRUCTURED-DATA part</summary>
        public StructuredData StructuredData { get; set; }

        /// <summary>Whether to remove or not BOM in the MSG part</summary>
        /// <see href="https://github.com/rsyslog/rsyslog/issues/284">RSyslog issue #284</see>
        public bool DisableBom { get; set; }

        /// <summary>Builds a new instance of the Rfc5424 class</summary>
        public Rfc5424()
        {
            defaultHostname = HostFqdn();
            defaultAppName = Assembly.GetCallingAssembly().GetName().Name;
            Version = DefaultVersion;
            Hostname = defaultHostname;
            AppName = defaultAppName;
            ProcId = NilValue;
            MsgId = NilValue;
            StructuredData = new StructuredData();
            DisableBom = false;
        }

        internal override void Initialize(Enforcement enforcement)
        {
            base.Initialize(enforcement);
            hostnamePolicySet = new FqdnHostnamePolicySet(enforcement, defaultHostname);
            appNamePolicySet = new AppNamePolicySet(enforcement, defaultAppName);
            procIdPolicySet = new ProcIdPolicySet(enforcement);
            msgIdPolicySet = new MsgIdPolicySet(enforcement);
            utf8MessagePolicy = new Utf8MessagePolicy(enforcement);
            StructuredData.Initialize(enforcement);
        }

        protected override ByteArray BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var encodings = new EncodingSet(!DisableBom);

            AppendHeaderBytes(pri, logEvent, encodings);
            Message.Append(SpaceBytes);
            AppendStructuredDataBytes(logEvent, encodings);
            Message.Append(SpaceBytes);
            AppendMsgBytes(logEntry, encodings);

            utf8MessagePolicy.Apply(Message);

            return Message;
        }

        private static string HostFqdn()
        {
            var hostname = Dns.GetHostName();
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var domainAsSuffix = $".{domainName}";
            return hostname.EndsWith(domainAsSuffix) ? hostname : $"{hostname}{domainAsSuffix}";
        }

        private void AppendHeaderBytes(string pri, LogEventInfo logEvent, EncodingSet encodings)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = hostnamePolicySet.Apply(Hostname.Render(logEvent));
            var appName = appNamePolicySet.Apply(AppName.Render(logEvent));
            var procId = procIdPolicySet.Apply(ProcId.Render(logEvent));
            var msgId = msgIdPolicySet.Apply(MsgId.Render(logEvent));
            var header = $"{pri}{Version} {timestamp} {hostname} {appName} {procId} {msgId}";
            var headerBytes = encodings.Ascii.GetBytes(header);
            Message.Append(headerBytes);
        }

        private void AppendStructuredDataBytes(LogEventInfo logEvent, EncodingSet encodings)
        {
            StructuredData.AppendBytes(Message, logEvent, encodings);
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