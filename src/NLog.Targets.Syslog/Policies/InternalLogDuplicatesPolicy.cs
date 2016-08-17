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
                .Select(g => g.Key)
                .Aggregate(string.Empty, (acc, cur) => $"{acc}, '{cur}'")
                .TrimStart(',', ' ');
            if (duplicates.Any())
                InternalLogger.Trace($"Found duplicates: {duplicates}");
            return list;
        }
    }
}