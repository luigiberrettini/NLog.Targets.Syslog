// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Common;

namespace NLog.Targets.Syslog.Policies
{
    internal class InternalLogDuplicatesPolicy
    {
        public bool IsApplicable()
        {
            return InternalLogger.IsTraceEnabled;
        }

        public void Apply<T>(IEnumerable<T> enumerable, Func<T, string> toBeCompared)
        {
            var duplicates = enumerable
                .GroupBy(toBeCompared)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .Aggregate(string.Empty, (acc, cur) => $"{acc}, '{cur}'")
                .TrimStart(',', ' ');

            if (duplicates.Any())
                InternalLogger.Trace("[Syslog] Found duplicates: {0}", duplicates);
        }
    }
}