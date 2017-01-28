// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class ParamNamePolicySet
    {
        private const string NonSafePrintUsAscii = @"[^\u0022\u003D\u005D\u0020-\u007E]";
        private const int ParamNameMaxLength = 32;
        private const string QuestionMark = "?";
        private readonly List<IBasicPolicy<string, string>> basicPolicies;
        private readonly ReplaceComputedValuePolicy replaceComputedValuePolicy;

        public ParamNamePolicySet(EnforcementConfig enforcementConfig)
        {
            basicPolicies = new List<IBasicPolicy<string, string>>
            {
                new TransliteratePolicy(enforcementConfig),
                new ReplaceKnownValuePolicy(enforcementConfig, NonSafePrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, ParamNameMaxLength)
            };
            replaceComputedValuePolicy = new ReplaceComputedValuePolicy(enforcementConfig, QuestionMark);
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