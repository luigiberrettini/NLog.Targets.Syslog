namespace NLog.Targets.Syslog.Policies
{
    public class Enforcement
    {
        /// <summary>Whether or not to split each log entry by newlines and send each line separately</summary>
        public bool SplitOnNewLine { get; set; }

        /// <summary>Whether or not to transliterate from Unicode to ASCII</summary>
        public bool Transliterate { get; set; }

        /// <summary>Whether or not to replace invalid characters on the basis of RFC rules</summary>
        public bool ReplaceInvalidCharacters { get; set; }

        /// <summary>Whether or not to truncate fields to the max length specified in RFCs</summary>
        public bool TruncateFieldsToMaxLength { get; set; }

        /// <summary>The length to truncate the Syslog message to or zero</summary>
        public int TruncateSyslogMessageTo { get; set; }
    }
}