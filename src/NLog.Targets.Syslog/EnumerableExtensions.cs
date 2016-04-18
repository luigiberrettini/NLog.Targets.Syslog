using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public static class EnumerableExtensions
    {
        /// <summary>Performs an action on each element of the enumerable</summary>
        /// <param name="enumerable">The enumerable on which to perform an action</param>
        /// <param name="action">The action to be performed</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
    }
}