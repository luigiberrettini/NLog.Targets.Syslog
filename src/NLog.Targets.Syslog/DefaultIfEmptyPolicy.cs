namespace NLog.Targets
{
    internal class DefaultIfEmptyPolicy : IBasicPolicy<string, string>
    {
        private readonly Enforcement enforcement;
        private readonly string defaultValue;

        public DefaultIfEmptyPolicy(Enforcement enforcement, string defaultValue)
        {
            this.enforcement = enforcement;
            this.defaultValue = defaultValue;
        }

        public bool IsApplicable()
        {
            return !string.IsNullOrEmpty(defaultValue);
        }

        public string Apply(string s)
        {
            return s.Length == 0 ? defaultValue : s;
        }
    }
}