using NLog.Common;

namespace NLog.Targets.Syslog.Policies
{
    internal class TruncateToComputedValuePolicy
    {
        private readonly int messageMaxLength;
        private readonly bool assumeAscii;

        public TruncateToComputedValuePolicy(Enforcement enforcement, bool assumeAsciiEncoding)
        {
            messageMaxLength = enforcement.TruncateMessageTo;
            assumeAscii = assumeAsciiEncoding;
        }

        public bool IsApplicable()
        {
            return messageMaxLength > 0;
        }

        public void Apply(ByteArray bytes)
        {
            var maxLength = messageMaxLength;

            if (maxLength <= 0 || maxLength >= bytes.Length)
                return;

            var computedMaxLength = MaxLengthToAvoidCharCorruption(bytes, maxLength);
            bytes.Resize(computedMaxLength);
            InternalLogger.Trace($"Truncated byte array to {computedMaxLength} bytes (truncateMessageTo {messageMaxLength})");
        }

        private int MaxLengthToAvoidCharCorruption(ByteArray bytes, int updatedMaxLength)
        {
            if (assumeAscii)
                return updatedMaxLength;

            var computedMaxLength = bytes.Length;
            for (var i = bytes.Length - 1; i >= 0; i--)
            {
                if (computedMaxLength <= updatedMaxLength && i.IsIndexOfCharTerminatingByte(bytes))
                    break;
                computedMaxLength--;
            }
            return computedMaxLength;
        }
    }
}