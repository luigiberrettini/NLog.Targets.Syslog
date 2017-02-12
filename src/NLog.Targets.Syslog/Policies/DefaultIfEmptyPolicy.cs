// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;

namespace NLog.Targets.Syslog.Policies
{
    internal class DefaultIfEmptyPolicy : IBasicPolicy<string, string>
    {
        private readonly string defaultValue;

        public DefaultIfEmptyPolicy(string defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public bool IsApplicable()
        {
            return !string.IsNullOrEmpty(defaultValue);
        }

        public string Apply(string s)
        {
            if (s.Length != 0)
                return s;

            InternalLogger.Trace(() => $"Applied default value '{defaultValue}'");
            return defaultValue;
        }
    }
}