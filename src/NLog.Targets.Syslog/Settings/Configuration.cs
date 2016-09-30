namespace NLog.Targets.Syslog.Settings
{
    /// <summary>SyslogTarget specific configuration</summary>
    public class Configuration
    {
        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public EnforcementConfig Enforcement { get; set; }

        /// <summary>Syslog message creation configuration</summary>
        public MessageBuilderConfig MessageCreation { get; set; }

        /// <summary>Syslog message send configuration</summary>
        public MessageTransmitterConfig MessageSend { get; set; }

        /// <summary>Builds a new instance of the Configuration class</summary>
        public Configuration()
        {
            Enforcement = new EnforcementConfig();
            MessageCreation = new MessageBuilderConfig();
            MessageSend = new MessageTransmitterConfig();
        }
    }
}