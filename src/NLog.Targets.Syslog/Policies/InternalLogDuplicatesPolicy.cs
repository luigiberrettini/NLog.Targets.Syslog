using NLog.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Targets.Syslog.MessageCreation;

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
                .Aggregate((current, next) => current + ", " + next);
            InternalLogger.Debug("Duplicates found: {duplicates}");
            return list;
        }
    }
}