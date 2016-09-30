using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class AppNamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int AppNameMaxLength = 48;

        public AppNamePolicySet(EnforcementConfig enforcementConfig, string defaultAppName)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new DefaultIfEmptyPolicy(defaultAppName),
                new ReplaceKnownValuePolicy(enforcementConfig, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, AppNameMaxLength)
            });
        }
    }
}