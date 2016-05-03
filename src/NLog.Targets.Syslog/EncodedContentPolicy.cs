using System.Collections.Generic;

namespace NLog.Targets
{
    internal class EncodedContentPolicy
    {
        private const bool AssumeAsciiEncoding = true;
        private readonly TruncateToComputedValuePolicy truncatePolicy;

        public EncodedContentPolicy(Enforcement initedEnforcement)
        {
            truncatePolicy = new TruncateToComputedValuePolicy(initedEnforcement, AssumeAsciiEncoding);
        }

        public IEnumerable<byte> Apply(byte[] bytes, int prefixLength)
        {
            return truncatePolicy.IsApplicable() ? truncatePolicy.Apply(bytes, prefixLength) : bytes;
        }
    }
}