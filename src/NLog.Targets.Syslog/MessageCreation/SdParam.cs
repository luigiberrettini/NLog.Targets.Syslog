// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.MessageStorage;
using NLog.Targets.Syslog.Policies;
using NLog.Targets.Syslog.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class SdParam
    {
        private static readonly byte[] SpaceBytes = { 0x20 };
        private static readonly byte[] EqualBytes = { 0x3D };
        private static readonly byte[] QuotesBytes = { 0x22 };

        private readonly Layout name;
        private readonly Layout value;
        private readonly ParamNamePolicySet paramNamePolicySet;
        private readonly ParamValuePolicySet paramValuePolicySet;

        public SdParam(SdParamConfig sdParamConfig, EnforcementConfig enforcementConfig)
        {
            name = sdParamConfig.Name;
            value = sdParamConfig.Value;
            paramNamePolicySet = new ParamNamePolicySet(enforcementConfig);
            paramValuePolicySet = new ParamValuePolicySet(enforcementConfig);
        }

        public static void Append(ByteArray message, List<SdParam> sdParams, LogEventInfo logEvent, string invalidNamesPattern)
        {
            foreach (var sdParam in sdParams)
            {
                message.AppendBytes(SpaceBytes);
                sdParam.Append(message, logEvent, invalidNamesPattern);
            }
        }

        public static string ToString(IEnumerable<SdParam> sdParams)
        {
            return sdParams.Aggregate(string.Empty, (acc, cur) => $"{acc} {cur.ToString()}");
        }

        public override string ToString()
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            return $"{name.Render(nullEvent)}=\"{value.Render(nullEvent)}\"";
        }

        private void Append(ByteArray message, LogEventInfo logEvent, string invalidNamesPattern)
        {
            AppendName(message, logEvent, invalidNamesPattern);
            message.AppendBytes(EqualBytes);
            message.AppendBytes(QuotesBytes);
            AppendValue(message, logEvent);
            message.AppendBytes(QuotesBytes);
        }

        private void AppendName(ByteArray message, LogEventInfo logEvent, string invalidNamesPattern)
        {
            var paramName = paramNamePolicySet.Apply(name.Render(logEvent), invalidNamesPattern);
            message.AppendAscii(paramName);
        }

        private void AppendValue(ByteArray message, LogEventInfo logEvent)
        {
            var paramValue = paramValuePolicySet.Apply(value.Render(logEvent));
            message.AppendUtf8(paramValue);
        }
    }
}