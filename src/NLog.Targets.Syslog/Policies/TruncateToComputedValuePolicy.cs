using NLog.Common;
using System;
using System.Collections.Generic;

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

        public byte[] Apply(byte[] bytes, int prefixLength)
        {
            var maxLength = messageMaxLength - prefixLength;

            if (maxLength <= 0 || bytes.Length <= maxLength)
                return bytes;

            var computedMaxLength = MaxLengthToAvoidCharCorruption(bytes, maxLength);
            Array.Resize(ref bytes, computedMaxLength);
            InternalLogger.Trace($"Truncated byte array to {computedMaxLength} bytes (truncateMessageTo {messageMaxLength} - prefixLength {prefixLength})");
            return bytes;
        }

        private int MaxLengthToAvoidCharCorruption(IReadOnlyList<byte> bytes, int updatedMaxLength)
        {
            if (assumeAscii)
                return updatedMaxLength;

            var computedMaxLength = bytes.Count;
            for (var i = bytes.Count - 1; i >= 0; i--)
            {
                if (computedMaxLength <= updatedMaxLength && i.IsIndexOfCharTerminatingByte(bytes))
                    break;
                computedMaxLength--;
            }
            return computedMaxLength;
        }
    }
}