namespace NLog.Targets.Syslog.Policies
{
    internal class AsciiMessagePolicy
    {
        private const bool AssumeAsciiEncoding = true;
        private readonly TruncateToComputedValuePolicy truncatePolicy;

        public AsciiMessagePolicy(Enforcement enforcement)
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