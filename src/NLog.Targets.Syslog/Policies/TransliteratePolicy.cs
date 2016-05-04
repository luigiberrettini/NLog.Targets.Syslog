using NLog.Common;
using UnidecodeSharpFork;

namespace NLog.Targets.Syslog.Policies
{
    internal class TransliteratePolicy : IBasicPolicy<string, string>
    {
        private readonly Enforcement enforcement;

        public TransliteratePolicy(Enforcement enforcement)
        {
            this.enforcement = enforcement;
        }

        public bool IsApplicable()
        {
            return enforcement.Transliterate;
        }

        public string Apply(string s)
        {
            if (s.Length == 0)
                return s;

            var unidecoded = s.Unidecode();
            InternalLogger.Trace($"Transliterated {s} to {unidecoded}");
            return unidecoded;
        }
    }
}