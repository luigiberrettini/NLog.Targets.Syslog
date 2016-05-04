using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;

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
        private MsgWithoutPreamblePolicy msgWithoutPreamblePolicy;
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

        /// <summary>Initializes the Rfc5424</summary>
        /// <param name="enforcement">The enforcement to apply</param>
        internal override void Initialize(Enforcement enforcement)
        {
            base.Initialize(enforcement);
            hostnamePolicySet = new FqdnHostnamePolicySet(enforcement, defaultHostname);
            appNamePolicySet = new AppNamePolicySet(enforcement, defaultAppName);
            procIdPolicySet = new ProcIdPolicySet(enforcement);
            msgIdPolicySet = new MsgIdPolicySet(enforcement);
            msgWithoutPreamblePolicy = new MsgWithoutPreamblePolicy(enforcement);
            StructuredData.Initialize(enforcement);
        }

        /// <summary>Builds the Syslog message according to the RFC</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="pri">The Syslog PRI part</param>
        /// <param name="logEntry">The entry to be logged</param>
        /// <returns>Bytes containing the Syslog message</returns>
        protected override IEnumerable<byte> BuildMessage(LogEventInfo logEvent, string pri, string logEntry)
        {
            var encodings = new EncodingSet(!DisableBom);

            var msgPrefixBytes = HeaderBytes(pri, logEvent, encodings)
                .Concat(SpaceBytes)
                .Concat(StructuredData.Bytes(logEvent, encodings))
                .Concat(SpaceBytes)
                .ToArray();
            var msgBytes = MsgBytes(logEntry, msgPrefixBytes.Length, encodings);
            return msgPrefixBytes.Concat(msgBytes);
        }

        private static string HostFqdn()
        {
            var hostname = Dns.GetHostName();
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var domainAsSuffix = $".{domainName}";
            return hostname.EndsWith(domainAsSuffix) ? hostname : $"{hostname}{domainAsSuffix}";
        }

        private IEnumerable<byte> HeaderBytes(string pri, LogEventInfo logEvent, EncodingSet encodings)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = hostnamePolicySet.Apply(Hostname.Render(logEvent));
            var appName = appNamePolicySet.Apply(AppName.Render(logEvent));
            var procId = procIdPolicySet.Apply(ProcId.Render(logEvent));
            var msgId = msgIdPolicySet.Apply(MsgId.Render(logEvent));
            var header = $"{pri}{Version} {timestamp} {hostname} {appName} {procId} {msgId}";
            return encodings.Ascii.GetBytes(header);
        }

        private IEnumerable<byte> MsgBytes(string logEntry, int msgPrefixLength, EncodingSet encodings)
        {
            var preambleBytes = encodings.Utf8.GetPreamble();
            var logEntryBytes = encodings.Utf8.GetBytes(logEntry);
            var msgWithoutPreamblePrefixLength = msgPrefixLength + preambleBytes.Length;
            var msgWithoutPreambleBytes = msgWithoutPreamblePolicy.Apply(logEntryBytes, msgWithoutPreamblePrefixLength);
            return preambleBytes.Concat(msgWithoutPreambleBytes);
        }
    }
}