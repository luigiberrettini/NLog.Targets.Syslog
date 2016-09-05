namespace NLog.Targets.Syslog.Policies
{
    internal class Utf8MessagePolicy
    {
        private const bool AssumeAsciiEncoding = false;
        private readonly TruncateToComputedValuePolicy truncatePolicy;

        public Utf8MessagePolicy(Enforcement enforcement)
        {
            truncatePolicy = new TruncateToComputedValuePolicy(enforcement, AssumeAsciiEncoding);
        }

        public void Apply(ByteArray bytes)
        {
            if (truncatePolicy.IsApplicable())
                truncatePolicy.Apply(bytes);
        }
    }
}