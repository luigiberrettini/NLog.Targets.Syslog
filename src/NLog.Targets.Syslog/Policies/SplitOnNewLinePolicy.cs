using NLog.Common;
using System;
using System.Collections.Generic;

namespace NLog.Targets.Syslog.Policies
{
    internal class SplitOnNewLinePolicy : IBasicPolicy<string, IEnumerable<string>>
    {
        private readonly Enforcement enforcement;
        private static readonly char[] LineSeps = { '\r', '\n' };

        public SplitOnNewLinePolicy(Enforcement enforcement)
        {
            this.enforcement = enforcement;
        }

        public bool IsApplicable()
        {
            return enforcement.SplitOnNewLine;
        }

        public IEnumerable<string> Apply(string s)
        {
            var split = s.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            InternalLogger.Trace($"Split {s} on new line");
            return split;
        }
    }
}