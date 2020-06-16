// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using NLog.Common;
using System.Text.RegularExpressions;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class ReplaceKnownValuePolicy : IBasicPolicy<string, string>
    {
        private readonly EnforcementConfig enforcementConfig;
        private readonly string searchFor;
        private readonly string replaceWith;
        private readonly bool applicabilityPreconditionsFailed;
        private readonly string logMsgStructure;

        public ReplaceKnownValuePolicy(EnforcementConfig enforcementConfig, string searchFor, string replaceWith)
        {
            this.enforcementConfig = enforcementConfig;
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
            applicabilityPreconditionsFailed = string.IsNullOrEmpty(searchFor) || replaceWith == null;
            // Prevents allocation of param array
            logMsgStructure = $"[Syslog] Replaced '{searchFor}' (if found) with '{replaceWith}' given computed value '{{0}}': '{{1}}'";
        }

        public bool IsApplicable()
        {
            return enforcementConfig.ReplaceInvalidCharacters && !string.IsNullOrEmpty(searchFor);
        }

        public string Apply(string s)
        {
            if (s.Length == 0 || applicabilityPreconditionsFailed)
                return s;

            var replaced = Regex.Replace(s, searchFor, replaceWith);
            if (!ReferenceEquals(replaced, s))
                InternalLogger.Trace(logMsgStructure, s, replaced);
            return replaced;
        }
    }
}