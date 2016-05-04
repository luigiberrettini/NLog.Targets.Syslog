using System.Collections.Generic;

namespace NLog.Targets
{
    internal class ParamValuePolicySet : PolicySet
    {
        private const string InvalidParamValuePattern = @"([^\\""\]]*)([\\""\]])([^\\""\]]*)";
        private const string InvalidParamValueReplacement = "$1\\$2$3";

        public ParamValuePolicySet(Enforcement initedEnforcement)
        {
            AddPolicies(new List<IBasicPolicy<string, string>>
            {
                new ReplaceKnownValuePolicy(initedEnforcement, InvalidParamValuePattern, InvalidParamValueReplacement)
            });
        }
    }
}