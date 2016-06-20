using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Collections.Generic;
using System.Linq;

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

        public override string ToString()
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            return $"{Name.Render(nullEvent)}=\"{Value.Render(nullEvent)}\"";
        }

        internal static IEnumerable<byte> Bytes(IEnumerable<SdParam> sdParams, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            return sdParams.SelectMany(sdParam => SpaceBytes.Concat(sdParam.Bytes(logEvent, invalidNamesPattern, encodings)));
        }

        private IEnumerable<byte> Bytes(LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            return NameBytes(logEvent, invalidNamesPattern, encodings)
                .Concat(EqualBytes)
                .Concat(QuotesBytes)
                .Concat(ValueBytes(logEvent, encodings))
                .Concat(QuotesBytes);
        }

        private IEnumerable<byte> NameBytes(LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            var paramName = paramNamePolicySet.Apply(Name.Render(logEvent), invalidNamesPattern);
            return encodings.Ascii.GetBytes(paramName);
        }

        private IEnumerable<byte> ValueBytes(LogEventInfo logEvent, EncodingSet encodings)
        {
            var paramValue = paramValuePolicySet.Apply(Value.Render(logEvent));
            return encodings.Utf8.GetBytes(paramValue);
        }
    }
}