// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Reflection;

namespace NLog.Targets.Syslog.Extensions
{
    internal static class AssemblyExtensions
    {
        public static string Name(this Assembly assembly)
        {
            return assembly.GetName().Name;
        }
    }
}