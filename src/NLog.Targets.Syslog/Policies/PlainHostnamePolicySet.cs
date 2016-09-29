using System.Net;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class PlainHostnamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";

        public PlainHostnamePolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new DefaultIfEmptyPolicy(Dns.GetHostName()),
                new ReplaceKnownValuePolicy(enforcementConfig, NonPrintUsAscii, QuestionMark)
            });
        }
    }
}