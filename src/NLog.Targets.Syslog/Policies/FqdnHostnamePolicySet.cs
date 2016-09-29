using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class FqdnHostnamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int HostnameMaxLength = 255;

        public FqdnHostnamePolicySet(EnforcementConfig enforcementConfig, string defaultHostname)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new DefaultIfEmptyPolicy(defaultHostname),
                new ReplaceKnownValuePolicy(enforcementConfig, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, HostnameMaxLength),
            });
        }
    }
}