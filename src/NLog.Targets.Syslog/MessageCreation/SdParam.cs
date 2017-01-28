// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.Settings;

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

        public static void AppendBytes(ByteArray message, IEnumerable<SdParam> sdParams, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            message.Append(SpaceBytes);
            sdParams.ForEach(sdParam => sdParam.AppendBytes(message, logEvent, invalidNamesPattern, encodings));
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

        private void AppendBytes(ByteArray message, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            AppendNameBytes(message, logEvent, invalidNamesPattern, encodings);
            message.Append(EqualBytes);
            message.Append(QuotesBytes);
            AppendValueBytes(message, logEvent, encodings);
            message.Append(QuotesBytes);
        }

        private void AppendNameBytes(ByteArray message, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            var paramName = paramNamePolicySet.Apply(name.Render(logEvent), invalidNamesPattern);
            var nameBytes = encodings.Ascii.GetBytes(paramName);
            message.Append(nameBytes);
        }

        private void AppendValueBytes(ByteArray message, LogEventInfo logEvent, EncodingSet encodings)
        {
            var paramValue = paramValuePolicySet.Apply(value.Render(logEvent));
            var valueBytes = encodings.Utf8.GetBytes(paramValue);
            message.Append(valueBytes);
        }
    }
}