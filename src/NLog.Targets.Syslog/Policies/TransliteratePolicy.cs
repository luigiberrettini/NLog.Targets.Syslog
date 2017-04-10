// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using NLog.Common;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class TransliteratePolicy : IBasicPolicy<string, string>
    {
        private readonly EnforcementConfig enforcementConfig;

        public TransliteratePolicy(EnforcementConfig enforcementConfig)
        {
            this.enforcementConfig = enforcementConfig;
        }

        public bool IsApplicable()
        {
            return enforcementConfig.Transliterate;
        }

        public string Apply(string s)
        {
            if (s.Length == 0)
                return s;

            var unidecoded = s.Unidecode();
            InternalLogger.Trace("Transliterated '{0}' to '{1}'", s, unidecoded);
            return unidecoded;
        }
    }
}