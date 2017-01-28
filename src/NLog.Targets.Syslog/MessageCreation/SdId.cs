// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class SdId
    {
        private readonly SimpleLayout layout;
        private readonly SdIdPolicySet sdIdPolicySet;

        public SdId(SimpleLayout sdIdConfig, EnforcementConfig enforcementConfig)
        {
            layout = sdIdConfig;
            sdIdPolicySet = new SdIdPolicySet(enforcementConfig);
        }

        public string Render(LogEventInfo logEvent)
        {
            return layout.Render(logEvent);
        }

        public void AppendBytes(ByteArray message, string renderedSdId, EncodingSet encodings)
        {
            var sdId = sdIdPolicySet.Apply(renderedSdId);
            var sdIdBytes = encodings.Ascii.GetBytes(sdId);
            message.Append(sdIdBytes);
        }
    }
}