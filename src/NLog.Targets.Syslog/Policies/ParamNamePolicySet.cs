using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.Policies
{
    internal class ParamNamePolicySet
    {
        private const string NonSafePrintUsAscii = @"[^\u0022\u003D\u005D\u0020-\u007E]";
        private const int ParamNameMaxLength = 32;
        private const string QuestionMark = "?";
        private readonly List<IBasicPolicy<string, string>> basicPolicies;
        private readonly ReplaceComputedValuePolicy replaceComputedValuePolicy;

        public ParamNamePolicySet(Enforcement enforcement)
        {
            basicPolicies = new List<IBasicPolicy<string, string>>
            {
                new TransliteratePolicy(enforcement),
                new ReplaceKnownValuePolicy(enforcement, NonSafePrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcement, ParamNameMaxLength)
            };
            replaceComputedValuePolicy = new ReplaceComputedValuePolicy(enforcement, QuestionMark);
        }

        public string Apply(string s, string searchFor)
        {
            var afterBasicPolicies = basicPolicies
                .Where(p => p.IsApplicable())
                .Aggregate(s, (acc, curr) => curr.Apply(acc));

            return replaceComputedValuePolicy.IsApplicable() ?
                replaceComputedValuePolicy.Apply(afterBasicPolicies, searchFor) :
                afterBasicPolicies;
        }
    }
}