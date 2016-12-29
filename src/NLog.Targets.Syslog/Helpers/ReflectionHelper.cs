using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NLog.Targets.Syslog.Helpers
{
    internal class ReflectionHelper
    {
        public static Assembly GetEntryAssembly()
        {
            var result = Assembly.GetEntryAssembly();
            if (result != null)
                return result;
            var mscorlib = typeof(object).Assembly;
            var stack = new StackTrace();
            Func<Assembly, bool> condition = (x) => !x.GlobalAssemblyCache && !x.FullName.StartsWith("Microsoft.");
            result = Enumerable.Range(0, stack.FrameCount)
                    .Reverse()
                    .Select(x => stack.GetFrame(x).GetMethod().DeclaringType.Assembly)
                    .SkipWhile(condition)
                    .First(condition);
            return result;
        }
    }
}
