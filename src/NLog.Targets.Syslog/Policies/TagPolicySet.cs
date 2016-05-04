namespace NLog.Targets.Syslog.Policies
{
    internal class TagPolicySet : PolicySet
    {
        private const string NonAlphaNumeric = @"[^a-zA-Z0-9]";
        private const string QuestionMark = "?";
        private const int TagMaxLength = 32;

        public TagPolicySet(Enforcement initedEnforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new ReplaceKnownValuePolicy(initedEnforcement, NonAlphaNumeric, QuestionMark),
                new TruncateToKnownValuePolicy(initedEnforcement, TagMaxLength),
            });
        }
    }
}