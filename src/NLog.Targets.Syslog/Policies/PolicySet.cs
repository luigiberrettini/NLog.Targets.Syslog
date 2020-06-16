// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;

namespace NLog.Targets.Syslog.Policies
{
    internal abstract class PolicySet
    {
        private readonly List<IBasicPolicy<string, string>> policies;

        protected PolicySet()
        {
            policies = new List<IBasicPolicy<string, string>>();
        }

        protected void AddPolicies(IEnumerable<IBasicPolicy<string, string>> policiesToAdd)
        {
            policies.AddRange(policiesToAdd);
        }

        public string Apply(string s)
        {
            var afterApplication = s;
            foreach (var policy in policies)
                if (policy.IsApplicable())
                    afterApplication = policy.Apply(afterApplication);
            return afterApplication;
        }
    }
}