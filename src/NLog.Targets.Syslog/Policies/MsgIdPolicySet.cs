namespace NLog.Targets.Syslog.Policies
{
    internal class MsgIdPolicySet : PolicySet
    {
        private const string NilValue = "-";
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int MsgIdMaxLength = 32;

        public MsgIdPolicySet(Enforcement enforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcement),
                new DefaultIfEmptyPolicy(NilValue),
                new ReplaceKnownValuePolicy(enforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcement, MsgIdMaxLength),
            });
        }
    }
}