using System.Collections.Generic;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal static class SdIdToInvalidParamNamePattern
    {
        private const string Timequality = "timeQuality";
        private const string Origin = "origin";
        private const string Meta = "meta";
        private static readonly string[] TimeQualityParamNames = { "tzKnown", "isSynced", "syncAccuracy" };
        private static readonly string[] OriginParamNames = { "ip", "enterpriseId", "software", "swVersion" };
        private static readonly string[] MetaParamNames = { "sequenceId", "sysUpTime", "language" };
        private const string InvalidIanaParamName = @"^(?!^(?:{0})$).*$";
        private static readonly Dictionary<string, string> InvalidIanaParamNames = new Dictionary<string, string>
        {
            { Timequality, BuildInvalidIanaParamNamePattern(TimeQualityParamNames) },
            { Origin, BuildInvalidIanaParamNamePattern(OriginParamNames) },
            { Meta, BuildInvalidIanaParamNamePattern(MetaParamNames) }
        };
        private const string NonSafePrintUsAscii = @"[^\u0022\u003D\u005D\u0020-\u007E]";
        private const string InvalidCustomParamName = NonSafePrintUsAscii;

        public static string Map(string sdId)
        {
            return InvalidIanaParamNames.ContainsKey(sdId) ? InvalidIanaParamNames[sdId] : InvalidCustomParamName;
        }

        private static string BuildInvalidIanaParamNamePattern(params string[] args)
        {
            var joined = string.Join("|", args);
            var formatted = string.Format(InvalidIanaParamName, joined);
            return formatted;
        }
    }
}