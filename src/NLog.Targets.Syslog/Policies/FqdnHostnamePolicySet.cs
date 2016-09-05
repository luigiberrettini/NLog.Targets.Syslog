namespace NLog.Targets.Syslog.Policies
{
    internal class FqdnHostnamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int HostnameMaxLength = 255;

        public FqdnHostnamePolicySet(Enforcement enforcement, string defaultHostname)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcement),
                new DefaultIfEmptyPolicy(defaultHostname),
                new ReplaceKnownValuePolicy(enforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcement, HostnameMaxLength),
            });
        }
    }
}