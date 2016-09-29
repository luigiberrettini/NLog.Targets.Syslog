using System.Collections.Generic;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class SdIdPolicySet : PolicySet
    {
        private const string Timequality = "timeQuality";
        private const string Origin = "origin";
        private const string Meta = "meta";
        private static readonly string ValidIanaId = $"^(?:{Timequality}|{Origin}|{Meta})$";
        private const string ValidCustomId = @"^(?:[\u0021\u0023-\u003C\u003E\u003F\u0041-\u005C\u005E-\u007E]*@[0-9](?:\.[0-9]+)*)$";
        private static readonly string InvalidSdId = $"^(?!(?:{ValidIanaId}|{ValidCustomId})).*$";
        private const string QuestionMark = "?";
        private const int SdIdMaxLength = 32;

        public SdIdPolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new List<IBasicPolicy<string, string>>
            {
                new TransliteratePolicy(enforcementConfig),
                new ReplaceKnownValuePolicy(enforcementConfig, InvalidSdId, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig,SdIdMaxLength)
            });
        }
    }
}