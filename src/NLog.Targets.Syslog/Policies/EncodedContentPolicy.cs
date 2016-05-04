using System.Collections.Generic;

namespace NLog.Targets.Syslog.Policies
{
    internal class EncodedContentPolicy
    {
        private const bool AssumeAsciiEncoding = true;
        private readonly TruncateToComputedValuePolicy truncatePolicy;

        public EncodedContentPolicy(Enforcement enforcement)
        {
            truncatePolicy = new TruncateToComputedValuePolicy(enforcement, AssumeAsciiEncoding);
        }

        public IEnumerable<byte> Apply(byte[] bytes, int prefixLength)
        {
            return truncatePolicy.IsApplicable() ? truncatePolicy.Apply(bytes, prefixLength) : bytes;
        }
    }
}