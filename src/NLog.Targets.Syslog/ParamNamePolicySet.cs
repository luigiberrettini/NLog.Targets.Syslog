namespace NLog.Targets
{
    internal class ParamNamePolicySet
    {
        private const int ParamNameMaxLength = 32;
        private const string QuestionMark = "?";
        private readonly ReplaceComputedValuePolicy replaceComputedValuePolicy;
        private readonly TruncateToKnownValuePolicy truncateToKnownValuePolicy;

        public ParamNamePolicySet(Enforcement initedEnforcement)
        {
            truncateToKnownValuePolicy = new TruncateToKnownValuePolicy(initedEnforcement, ParamNameMaxLength);
            replaceComputedValuePolicy = new ReplaceComputedValuePolicy(initedEnforcement, QuestionMark);
        }
        public string Apply(string s, string replaceWith)
        {
            var a = truncateToKnownValuePolicy.IsApplicable() ? truncateToKnownValuePolicy.Apply(s) : s;
            var b = replaceComputedValuePolicy.IsApplicable() ? replaceComputedValuePolicy.Apply(a, replaceWith) : a;
            return b;
        }
    }
}