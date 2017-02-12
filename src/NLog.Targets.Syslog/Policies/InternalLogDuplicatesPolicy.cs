// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.Policies
{
    internal static class InternalLogDuplicatesPolicy
    {
        public static void Apply<T>(IEnumerable<T> enumerable, Func<T, string> toBeCompared)
        {
            var duplicates = enumerable
                .GroupBy(toBeCompared)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .Aggregate(string.Empty, (acc, cur) => $"{acc}, '{cur}'")
                .TrimStart(',', ' ');

            if (duplicates.Any())
                InternalLogger.Trace(() => $"Found duplicates: {duplicates}");
        }
    }
}