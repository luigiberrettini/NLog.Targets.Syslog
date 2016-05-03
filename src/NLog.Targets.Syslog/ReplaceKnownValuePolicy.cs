using System.Text.RegularExpressions;

namespace NLog.Targets
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
            return s.Length == 0 ? s : Regex.Replace(s, searchFor, replaceWith);
        }
    }
}