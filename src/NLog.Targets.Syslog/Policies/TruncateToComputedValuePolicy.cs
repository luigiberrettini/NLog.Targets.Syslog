using NLog.Common;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class TruncateToComputedValuePolicy
    {
        // RFC 5426 (UDP/IPv6) with jumbograms: (2^32 - 1) - 40 - 8 = 4294967247
        private const long MaxLengthNotToBeExceeded = 4294967247;
        private readonly long messageMaxLength;
        private readonly bool assumeAscii;

        public TruncateToComputedValuePolicy(EnforcementConfig enforcementConfig, bool assumeAsciiEncoding)
        {
            messageMaxLength = enforcementConfig.TruncateMessageTo > MaxLengthNotToBeExceeded ? MaxLengthNotToBeExceeded : enforcementConfig.TruncateMessageTo;
            assumeAscii = assumeAsciiEncoding;
        }

        public bool IsApplicable()
        {
            return messageMaxLength > 0;
        }

        public void Apply(ByteArray bytes)
        {
            var maxLength = messageMaxLength;

            if (maxLength <= bytes.EmptyContentLength || maxLength >= bytes.Length)
                return;

            var computedMaxLength = MaxLengthToAvoidCharCorruption(bytes, maxLength);
            bytes.Resize(computedMaxLength);
            InternalLogger.Trace($"Truncated byte array to {computedMaxLength} bytes (truncateMessageTo {messageMaxLength})");
        }

        private long MaxLengthToAvoidCharCorruption(ByteArray bytes, long updatedMaxLength)
        {
            if (assumeAscii)
                return updatedMaxLength;

            var bytesLength = bytes.Length;
            var computedMaxLength = bytesLength;
            for (var i = bytesLength - 1; i >= 0; i--)
            {
                if (computedMaxLength <= updatedMaxLength && i.IsIndexOfCharTerminatingByte(bytes))
                    break;
                computedMaxLength--;
            }
            return computedMaxLength;
        }
    }
}