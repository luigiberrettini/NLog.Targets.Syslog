using System.Collections.Generic;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class ByteManipulationExtensions
    {
        public static void ShiftBytesRight(this IList<byte> buffer, int oldLength, int newLength)
        {
            var idxOld = oldLength - 1;
            var idxNew = newLength - 1;
            while (idxOld >= 0)
                buffer[idxNew--] = buffer[idxOld--];
        }

        public static bool IsIndexOfCharTerminatingByte(this int i, ByteArray bytes)
        {
            return i.IsLastIndex(bytes) ||
                   bytes[i].IsSingleByte() ||
                   (bytes[i].IsContinuationByte() && bytes[i + 1].IsNonContinuationByte());
        }

        private static bool IsLastIndex(this int i, ByteArray bytes)
        {
            return i == bytes.Length - 1;
        }

        private static bool IsSingleByte(this byte b)
        {
            var topTwoBits = OnlyTopTwoBitsPreserved(b);
            return topTwoBits == 0x00 || topTwoBits == 0x40;
        }

        private static bool IsContinuationByte(this byte b)
        {
            return !IsNonContinuationByte(b);
        }

        private static bool IsNonContinuationByte(this byte b)
        {
            return OnlyTopTwoBitsPreserved(b) != 0x80;
        }

        private static int OnlyTopTwoBitsPreserved(byte b)
        {
            return b & 0xc0;
        }
    }
}