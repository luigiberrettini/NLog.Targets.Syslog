using System.Net;

namespace NLog.Targets.Syslog.Policies
{
    internal class PlainHostnamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";

        public PlainHostnamePolicySet(Enforcement enforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcement),
                new DefaultIfEmptyPolicy(enforcement, Dns.GetHostName()),
                new ReplaceKnownValuePolicy(enforcement, NonPrintUsAscii, QuestionMark)
            });
        }
    }
}