// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class StackTraceExtensions
    {
        private const string NLogAssemblyName = "NLog";

        public static Assembly EntryAssembly(this StackTrace stackTrace)
        {
            return stackTrace
                .GetFrames()
                ?.Select(x => x.GetMethod().DeclaringType?.Assembly)
                .Where(x => x != null)
                .SkipWhile(NotNLog)
                .First(NotNLog);
        }

        private static bool NotNLog(Assembly x)
        {
            return x.GetName().Name != NLogAssemblyName;
        }
    }
}