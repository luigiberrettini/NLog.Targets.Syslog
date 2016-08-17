using NLog.Common;

namespace NLog.Targets.Syslog.Policies
{
    internal class TruncateToKnownValuePolicy : IBasicPolicy<string, string>
    {
        private readonly Enforcement enforcement;
        private readonly int maxLength;

        public TruncateToKnownValuePolicy(Enforcement enforcement, int maxLength)
        {
            this.enforcement = enforcement;
            this.maxLength = maxLength;
        }

        public bool IsApplicable()
        {
            return enforcement.TruncateFieldsToMaxLength && maxLength > 0;
        }

        public string Apply(string s)
        {
            if (s.Length <= maxLength)
                return s;

            var truncated = s.Substring(0, maxLength);
            InternalLogger.Trace($"Truncated {s} to {maxLength}: {truncated}");
            return truncated;
        }
    }
}