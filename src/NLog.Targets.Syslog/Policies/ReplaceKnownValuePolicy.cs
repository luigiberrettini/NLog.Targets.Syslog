using NLog.Common;
using System.Text.RegularExpressions;

namespace NLog.Targets.Syslog.Policies
{
    internal class ReplaceKnownValuePolicy : IBasicPolicy<string, string>
    {
        private readonly Enforcement enforcement;
        private readonly string searchFor;
        private readonly string replaceWith;

        public ReplaceKnownValuePolicy(Enforcement enforcement, string searchFor, string replaceWith)
        {
            this.enforcement = enforcement;
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
        }

        public bool IsApplicable()
        {
            return enforcement.ReplaceInvalidCharacters && !string.IsNullOrEmpty(searchFor);
        }

        public string Apply(string s)
        {
            if (s.Length == 0)
                return s;

            var replaced = Regex.Replace(s, searchFor, replaceWith);
            InternalLogger.Trace($"Replaced '{searchFor}' (if found) with '{replaceWith}' given known value '{s}': '{replaced}'");
            return replaced;
        }
    }
}