// Licensed under the BSD license
// See the LICENSE file in the project root for more information

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

        public ReplaceKnownValuePolicy(EnforcementConfig enforcementConfig, string searchFor, string replaceWith)
        {
            this.enforcementConfig = enforcementConfig;
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
        }

        public bool IsApplicable()
        {
            return enforcementConfig.ReplaceInvalidCharacters && !string.IsNullOrEmpty(searchFor);
        }

        public string Apply(string s)
        {
            if (s.Length == 0)
                return s;

            var replaced = Regex.Replace(s, searchFor, replaceWith);
            InternalLogger.Trace(() => $"Replaced '{searchFor}' (if found) with '{replaceWith}' given known value '{s}': '{replaced}'");
            return replaced;
        }
    }
}