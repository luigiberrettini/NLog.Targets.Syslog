namespace NLog.Targets
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
            return s.Length <= maxLength ? s : s.Substring(0, maxLength);

        }
    }
}