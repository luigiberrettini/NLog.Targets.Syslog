using System.Collections.Generic;

namespace NLog.Targets.Syslog.Policies
{
    internal static class ByteManipulationExtension
    {
        public static bool IsIndexOfCharTerminatingByte(this int i, IReadOnlyList<byte> bytes)
        {
            return bytes[i].IsSingleByte() ||
                   bytes[i].IsContinuationByte() && (i.IsLastIndex(bytes) || bytes[i + 1].IsNonContinuationByte());
        }

        private static bool IsLastIndex(this int i, IReadOnlyCollection<byte> bytes)
        {
            return i == bytes.Count;
        }

        private static bool IsSingleByte(this byte b)
        {
            return OnlyTopTwoBitsPreserved(b) == 0x00;
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