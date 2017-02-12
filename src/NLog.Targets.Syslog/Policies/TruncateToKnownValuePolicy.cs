// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class TruncateToKnownValuePolicy : IBasicPolicy<string, string>
    {
        private readonly EnforcementConfig enforcementConfig;
        private readonly int maxLength;

        public TruncateToKnownValuePolicy(EnforcementConfig enforcementConfig, int maxLength)
        {
            this.enforcementConfig = enforcementConfig;
            this.maxLength = maxLength;
        }

        public bool IsApplicable()
        {
            return enforcementConfig.TruncateFieldsToMaxLength && maxLength > 0;
        }

        public string Apply(string s)
        {
            if (s.Length <= maxLength)
                return s;

            var truncated = s.Substring(0, maxLength);
            InternalLogger.Trace(() => $"Truncated '{s}' to {maxLength} characters: '{truncated}'");
            return truncated;
        }
    }
}