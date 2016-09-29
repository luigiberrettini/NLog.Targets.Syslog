namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Enforcement configuration</summary>
    public class EnforcementConfig
    {
        /// <summary>Throttling to be triggered when a configured number of log entries are waiting to be processed</summary>
        public ThrottlingConfig Throttling { get; set; }

        /// <summary>Whether or not to split each log entry by newlines and send each line separately</summary>
        public bool SplitOnNewLine { get; set; }

        /// <summary>Whether or not to transliterate from Unicode to ASCII</summary>
        public bool Transliterate { get; set; }

        /// <summary>Whether or not to replace invalid characters on the basis of RFC rules</summary>
        public bool ReplaceInvalidCharacters { get; set; }

        /// <summary>Whether or not to truncate fields to the max length specified in RFCs</summary>
        public bool TruncateFieldsToMaxLength { get; set; }

        /// <summary>The length to truncate the Syslog message to or zero</summary>
        public long TruncateMessageTo { get; set; }

        public EnforcementConfig()
        {
            Throttling = new ThrottlingConfig();
        }
    }
}