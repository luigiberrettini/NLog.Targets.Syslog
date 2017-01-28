// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class ParamValuePolicySet : PolicySet
    {
        private const string InvalidParamValuePattern = @"([^\\""\]]*)([\\""\]])([^\\""\]]*)";
        private const string InvalidParamValueReplacement = "$1\\$2$3";

        public ParamValuePolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new List<IBasicPolicy<string, string>>
            {
                new ReplaceKnownValuePolicy(enforcementConfig, InvalidParamValuePattern, InvalidParamValueReplacement)
            });
        }
    }
}