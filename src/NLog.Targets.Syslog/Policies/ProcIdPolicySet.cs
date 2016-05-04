namespace NLog.Targets
{
    internal class ProcIdPolicySet : PolicySet
    {
        private const string NilValue = "-";
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int ProcIdMaxLength = 128;

        public ProcIdPolicySet(Enforcement initedEnforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new DefaultIfEmptyPolicy(initedEnforcement, NilValue),
                new ReplaceKnownValuePolicy(initedEnforcement, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(initedEnforcement, ProcIdMaxLength),
            });
        }
    }
}