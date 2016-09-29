using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class TagPolicySet : PolicySet
    {
        private const string NonAlphaNumeric = @"[^a-zA-Z0-9]";
        private const string QuestionMark = "?";
        private const int TagMaxLength = 32;

        public TagPolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new ReplaceKnownValuePolicy(enforcementConfig, NonAlphaNumeric, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, TagMaxLength),
            });
        }
    }
}