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