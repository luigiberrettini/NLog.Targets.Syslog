namespace NLog.Targets.Syslog.Policies
{
    internal class FqdnHostnamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int HostnameMaxLength = 255;

        public FqdnHostnamePolicySet(Enforcement initedEnforcement, string defaultHostname)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new DefaultIfEmptyPolicy(initedEnforcement, defaultHostname),
                new ReplaceKnownValuePolicy(initedEnforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(initedEnforcement, HostnameMaxLength),
            });
        }
    }
}