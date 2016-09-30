using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class MsgIdPolicySet : PolicySet
    {
        private const string NilValue = "-";
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int MsgIdMaxLength = 32;

        public MsgIdPolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new DefaultIfEmptyPolicy(NilValue),
                new ReplaceKnownValuePolicy(enforcementConfig, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, MsgIdMaxLength)
            });
        }
    }
}