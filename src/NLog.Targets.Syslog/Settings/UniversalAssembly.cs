// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Diagnostics;
using System.Reflection;
using NLog.Targets.Syslog.Extensions;

namespace NLog.Targets.Syslog.Settings
{
    internal static class UniversalAssembly
    {
        public static Assembly EntryAssembly()
        {
            return Assembly.GetEntryAssembly() ??
                new StackTrace().EntryAssembly() ??
                Assembly.GetExecutingAssembly();
        }
    }
}