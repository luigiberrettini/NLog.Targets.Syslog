// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Enforcement configuration</summary>
    public class EnforcementConfig : NotifyPropertyChanged, IDisposable
    {
        private ThrottlingConfig throttling;
        private readonly PropertyChangedEventHandler throttlingPropsChanged;
        private int messageProcessors;
        private bool splitOnNewLine;
        private bool transliterate;
        private bool replaceInvalidCharacters;
        private bool truncateFieldsToMaxLength;
        private long truncateMessageTo;

        /// <summary>Throttling to be triggered when a configured number of log entries are waiting to be processed</summary>
        public ThrottlingConfig Throttling
        {
            get { return throttling; }
            set { SetProperty(ref throttling, value); }
        }

        /// <summary>The amount of parallel message processors</summary>
        public int MessageProcessors
        {
            get { return messageProcessors; }
            set { SetProperty(ref messageProcessors, value <= 0 ? Environment.ProcessorCount : value); }
        }

        /// <summary>Whether or not to split each log entry by newlines and send each line separately</summary>
        public bool SplitOnNewLine
        {
            get { return splitOnNewLine; }
            set { SetProperty(ref splitOnNewLine, value); }
        }

        /// <summary>Whether or not to transliterate from Unicode to ASCII</summary>
        public bool Transliterate
        {
            get { return transliterate; }
            set { SetProperty(ref transliterate, value); }
        }

        /// <summary>Whether or not to replace invalid characters on the basis of RFC rules</summary>
        public bool ReplaceInvalidCharacters
        {
            get { return replaceInvalidCharacters; }
            set { SetProperty(ref replaceInvalidCharacters, value); }
        }

        /// <summary>Whether or not to truncate fields to the max length specified in RFCs</summary>
        public bool TruncateFieldsToMaxLength
        {
            get { return truncateFieldsToMaxLength; }
            set { SetProperty(ref truncateFieldsToMaxLength, value); }
        }

        /// <summary>The length to truncate the Syslog message to or zero</summary>
        public long TruncateMessageTo
        {
            get { return truncateMessageTo; }
            set { SetProperty(ref truncateMessageTo, value); }
        }

        /// <summary>Builds a new instance of the EnforcementConfig class</summary>
        public EnforcementConfig()
        {
            throttling = new ThrottlingConfig();
            throttlingPropsChanged = (sender, args) => OnPropertyChanged(nameof(Throttling));
            throttling.PropertyChanged += throttlingPropsChanged;
            messageProcessors = 1;
        }

        /// <summary>Disposes the instance</summary>
        public void Dispose()
        {
            throttling.PropertyChanged -= throttlingPropsChanged;
        }
    }
}