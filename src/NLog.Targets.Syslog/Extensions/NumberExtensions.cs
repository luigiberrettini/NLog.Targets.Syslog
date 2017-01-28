// Licensed under the BSD license
// See the LICENSE file in the project root for more information

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

        public static IEnumerable<T> Select<T>(this int n, Func<int, T> func)
        {
            return Enumerable.Range(0, n).Select(func);
        }
    }
}