namespace NLog.Targets
{
    internal class AppNamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int AppNameMaxLength = 48;

        public AppNamePolicySet(Enforcement initedEnforcement, string defaultAppName)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new DefaultIfEmptyPolicy(initedEnforcement, defaultAppName),
                new ReplaceKnownValuePolicy(initedEnforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(initedEnforcement, AppNameMaxLength),
            });
        }
    }
}