using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> actual, Action<T> action)
        {
            foreach (var item in actual)
                action(item);
        }
    }
}