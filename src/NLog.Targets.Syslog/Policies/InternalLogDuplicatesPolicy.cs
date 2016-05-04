using NLog.Common;
using NLog.Targets.Syslog.MessageCreation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.Policies
{
    internal class InternalLogDuplicatesPolicy
    {
        public static IEnumerable<SdId> Apply(IEnumerable<SdId> enumerable, Func<SdId, string> toBeCompared)
        {
            var list = enumerable.ToList();
            var duplicates = list
                .GroupBy(toBeCompared)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .Aggregate(string.Empty, (acc, cur) => acc + ", " + cur);
            if (duplicates.Any())
                InternalLogger.Trace($"Duplicates found: {duplicates}");
            return list;
        }
    }
}