using System.Collections.Generic;

namespace NLog.Targets.Syslog.Policies
{
    internal class PlainContentPolicySet : PolicySet
    {
        private const string NonSpaceOrPrintUsAscii = @"[^\u0020-\u007E]";
        private const string QuestionMark = "?";
        private const string NonAlphaNumericFirstChar = @"^([a-zA-Z0-9])(.*)$";
        private const string PrefixWithSpaceReplacement = " $1$2";

        public PlainContentPolicySet(Enforcement enforcement)
        {
            AddPolicies(new List<IBasicPolicy<string, string>>
            {
                new TransliteratePolicy(enforcement),
                new ReplaceKnownValuePolicy(enforcement, NonSpaceOrPrintUsAscii, QuestionMark),
                new ReplaceKnownValuePolicy(enforcement, NonAlphaNumericFirstChar, PrefixWithSpaceReplacement),
            });
        }
    }
}