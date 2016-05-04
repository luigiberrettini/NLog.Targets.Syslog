using System.Collections.Generic;

namespace NLog.Targets.Syslog.Policies
{
    internal class MsgWithoutPreamblePolicy
    {
        private const bool AssumeAsciiEncoding = false;
        private readonly TruncateToComputedValuePolicy truncatePolicy;

        public MsgWithoutPreamblePolicy(Enforcement initedEnforcement)
        {
            truncatePolicy = new TruncateToComputedValuePolicy(initedEnforcement, AssumeAsciiEncoding);
        }

        public IEnumerable<byte> Apply(byte[] bytes, int prefixLength)
        {
            return truncatePolicy.IsApplicable() ? truncatePolicy.Apply(bytes, prefixLength) : bytes;
        }
    }
}