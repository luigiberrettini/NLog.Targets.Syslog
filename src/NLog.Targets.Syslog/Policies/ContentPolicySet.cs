// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class ContentPolicySet : PolicySet
    {
        private const string NonSpaceOrPrintUsAscii = @"[^\u0020-\u007E]";
        private const string QuestionMark = "?";
        private const string AlphaNumericFirstChar = @"^([a-zA-Z0-9])(.*)$";
        private const string PrefixWithSpaceReplacement = " $1$2"; // content first char must be non alphanumeric

        public ContentPolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new List<IBasicPolicy<string, string>>
            {
                new TransliteratePolicy(enforcementConfig),
                new ReplaceKnownValuePolicy(enforcementConfig, NonSpaceOrPrintUsAscii, QuestionMark),
                new ReplaceKnownValuePolicy(enforcementConfig, AlphaNumericFirstChar, PrefixWithSpaceReplacement)
            });
        }
    }
}