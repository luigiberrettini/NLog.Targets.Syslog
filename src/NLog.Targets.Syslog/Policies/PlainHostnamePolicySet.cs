using System.Net;

namespace NLog.Targets.Syslog.Policies
{
    internal class PlainHostnamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";

        public PlainHostnamePolicySet(Enforcement initedEnforcement)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new DefaultIfEmptyPolicy(initedEnforcement, Dns.GetHostName()),
                new ReplaceKnownValuePolicy(initedEnforcement, NonPrintUsAscii, QuestionMark)
            });
        }
    }
}