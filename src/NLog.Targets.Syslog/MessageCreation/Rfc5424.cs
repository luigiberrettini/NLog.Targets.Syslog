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
        private readonly byte[] preamble;
        private readonly FqdnHostnamePolicySet hostnamePolicySet;
        private readonly AppNamePolicySet appNamePolicySet;
        private readonly ProcIdPolicySet procIdPolicySet;
        private readonly MsgIdPolicySet msgIdPolicySet;
        private readonly Utf8MessagePolicy utf8MessagePolicy;

        public Rfc5424(Facility facility, LogLevelSeverityConfig logLevelSeverityConfig, Rfc5424Config rfc5424Config, EnforcementConfig enforcementConfig) : base(facility, logLevelSeverityConfig, enforcementConfig)
        {
            version = rfc5424Config.Version;
            hostnameLayout = rfc5424Config.Hostname;
            appNameLayout = rfc5424Config.AppName;
            procIdLayout = rfc5424Config.ProcId;
            msgIdLayout = rfc5424Config.MsgId;
            structuredData = new StructuredData(rfc5424Config.StructuredData, enforcementConfig);
            preamble = rfc5424Config.DisableBom ? new byte[0] : Encoding.UTF8.GetPreamble();
            hostnamePolicySet = new FqdnHostnamePolicySet(enforcementConfig, rfc5424Config.DefaultHostname);
            appNamePolicySet = new AppNamePolicySet(enforcementConfig, rfc5424Config.DefaultAppName);
            procIdPolicySet = new ProcIdPolicySet(enforcementConfig);
            msgIdPolicySet = new MsgIdPolicySet(enforcementConfig);
            utf8MessagePolicy = new Utf8MessagePolicy(enforcementConfig);
        }

        protected override void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string pri, string logEntry)
        {
            AppendHeader(buffer, pri, logEvent);
            buffer.AppendBytes(SpaceBytes);
            AppendStructuredData(buffer, logEvent);
            buffer.AppendBytes(SpaceBytes);
            AppendMsg(buffer, logEntry);

            utf8MessagePolicy.Apply(buffer);
        }

        private void AppendHeader(ByteArray buffer, string pri, LogEventInfo logEvent)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = hostnamePolicySet.Apply(hostnameLayout.Render(logEvent));
            var appName = appNamePolicySet.Apply(appNameLayout.Render(logEvent));
            var procId = procIdPolicySet.Apply(procIdLayout.Render(logEvent));
            var msgId = msgIdPolicySet.Apply(msgIdLayout.Render(logEvent));

            buffer.AppendAscii(pri);
            buffer.AppendAscii(version);
            buffer.AppendBytes(SpaceBytes);
            buffer.AppendAscii(timestamp);
            buffer.AppendBytes(SpaceBytes);
            buffer.AppendAscii(hostname);
            buffer.AppendBytes(SpaceBytes);
            buffer.AppendAscii(appName);
            buffer.AppendBytes(SpaceBytes);
            buffer.AppendAscii(procId);
            buffer.AppendBytes(SpaceBytes);
            buffer.AppendAscii(msgId);
        }

        private void AppendStructuredData(ByteArray buffer, LogEventInfo logEvent)
        {
            structuredData.Append(buffer, logEvent);
        }

        private void AppendMsg(ByteArray buffer, string logEntry)
        {
            buffer.AppendBytes(preamble);
            buffer.AppendUtf8(logEntry);
        }
    }
}