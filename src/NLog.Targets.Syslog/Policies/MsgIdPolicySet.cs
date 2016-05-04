namespace NLog.Targets.Syslog.Policies
{
    internal class MsgIdPolicySet : PolicySet
    {
        private const string NilValue = "-";
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int MsgIdMaxLength = 32;

        public MsgIdPolicySet(Enforcement initedEnforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new DefaultIfEmptyPolicy(initedEnforcement, NilValue),
                new ReplaceKnownValuePolicy(initedEnforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(initedEnforcement, MsgIdMaxLength),
            });
        }
    }
}