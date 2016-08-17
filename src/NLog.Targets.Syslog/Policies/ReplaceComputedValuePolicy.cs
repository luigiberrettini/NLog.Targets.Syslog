using NLog.Common;
using System.Text.RegularExpressions;

namespace NLog.Targets.Syslog.Policies
{
    internal class ReplaceComputedValuePolicy
    {
        private readonly Enforcement enforcement;
        private readonly string replaceWith;

        public ReplaceComputedValuePolicy(Enforcement enforcement, string replaceWith)
        {
            this.enforcement = enforcement;
            this.replaceWith = replaceWith;
        }

        public bool IsApplicable()
        {
            return enforcement.ReplaceInvalidCharacters;
        }

        public string Apply(string s, string searchFor)
        {
            if (string.IsNullOrEmpty(searchFor) || string.IsNullOrEmpty(replaceWith) || s.Length == 0)
                return s;

            var replaced = Regex.Replace(s, searchFor, replaceWith);
            InternalLogger.Trace($"Replaced '{searchFor}' (if found) with '{replaceWith}' given computed value '{s}': '{replaced}'");
            return replaced;
        }
    }
}