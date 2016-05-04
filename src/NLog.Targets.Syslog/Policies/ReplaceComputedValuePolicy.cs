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
            return string.IsNullOrEmpty(searchFor) || s.Length == 0 ? s : Regex.Replace(s, searchFor, replaceWith);
        }
    }
}