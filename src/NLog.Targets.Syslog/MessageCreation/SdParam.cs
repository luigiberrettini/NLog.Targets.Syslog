using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog.MessageCreation
{
    /// <summary>A Syslog SD-PARAM field</summary>
    [NLogConfigurationItem]
    public class SdParam
    {
        private ParamNamePolicySet paramNamePolicySet;
        private ParamValuePolicySet paramValuePolicySet;
        private static readonly byte[] SpaceBytes = { 0x20 };
        private static readonly byte[] EqualBytes = { 0x3D };
        private static readonly byte[] QuotesBytes = { 0x22 };

        /// <summary>The PARAM-NAME field of this SD-PARAM</summary>
        public Layout Name { get; set; }

        /// <summary>The PARAM-VALUE field of this SD-PARAM</summary>
        public Layout Value { get; set; }

        internal void Initialize(Enforcement enforcement)
        {
            paramNamePolicySet = new ParamNamePolicySet(enforcement);
            paramValuePolicySet = new ParamValuePolicySet(enforcement);
        }

        internal static string ToString(IEnumerable<SdParam> sdParams)
        {
            return sdParams.Aggregate(string.Empty, (acc, cur) => $"{acc} {cur.ToString()}");
        }

        /// <summary>Gives a string representation of an SdParam instance</summary>
        public override string ToString()
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            return $"{Name.Render(nullEvent)}=\"{Value.Render(nullEvent)}\"";
        }

        internal static void AppendBytes(ByteArray message, IEnumerable<SdParam> sdParams, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            message.Append(SpaceBytes);
            sdParams.ForEach(sdParam => sdParam.AppendBytes(message, logEvent, invalidNamesPattern, encodings));
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
            var paramName = paramNamePolicySet.Apply(Name.Render(logEvent), invalidNamesPattern);
            var nameBytes = encodings.Ascii.GetBytes(paramName);
            message.Append(nameBytes);
        }

        private void AppendValueBytes(ByteArray message, LogEventInfo logEvent, EncodingSet encodings)
        {
            var paramValue = paramValuePolicySet.Apply(Value.Render(logEvent));
            var valueBytes = encodings.Utf8.GetBytes(paramValue);
            message.Append(valueBytes);
        }
    }
}