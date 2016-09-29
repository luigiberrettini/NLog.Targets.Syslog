using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class NumberExtensions
    {
        public static void ForEach(this int n, Action<int> action)
        {
            for (var i = 0; i < n; i++)
                action(i);
        }

        public static IEnumerable<T> Select<T>(this int n, Func<T> func)
        {
            return Enumerable.Range(0, n).Select(i => func());
        }
    }
}