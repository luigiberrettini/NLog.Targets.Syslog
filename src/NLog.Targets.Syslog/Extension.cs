using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    internal static class Extension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

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