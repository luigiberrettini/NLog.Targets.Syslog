namespace NLog.Targets.Syslog.Policies
{
    internal class AppNamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int AppNameMaxLength = 48;

        public AppNamePolicySet(Enforcement enforcement, string defaultAppName)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcement),
                new DefaultIfEmptyPolicy(defaultAppName),
                new ReplaceKnownValuePolicy(enforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcement, AppNameMaxLength),
            });
        }
    }
}