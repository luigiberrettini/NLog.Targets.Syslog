namespace NLog.Targets.Syslog.Policies
{
    internal class TagPolicySet : PolicySet
    {
        private const string NonAlphaNumeric = @"[^a-zA-Z0-9]";
        private const string QuestionMark = "?";
        private const int TagMaxLength = 32;

        public TagPolicySet(Enforcement enforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcement),
                new ReplaceKnownValuePolicy(enforcement, NonAlphaNumeric, QuestionMark),
                new TruncateToKnownValuePolicy(enforcement, TagMaxLength),
            });
        }
    }
}