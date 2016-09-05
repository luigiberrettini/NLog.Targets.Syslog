namespace NLog.Targets.Syslog.Policies
{
    internal class ProcIdPolicySet : PolicySet
    {
        private const string NilValue = "-";
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int ProcIdMaxLength = 128;

        public ProcIdPolicySet(Enforcement enforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcement),
                new DefaultIfEmptyPolicy(NilValue),
                new ReplaceKnownValuePolicy(enforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcement, ProcIdMaxLength),
            });
        }
    }
}