namespace NLog.Targets.Syslog.Settings
{
    /// <summary>SyslogTarget specific configuration</summary>
    public class Configuration
    {
        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public EnforcementConfig Enforcement { get; set; }

        /// <summary>The builder used to create messages according to RFCs</summary>
        public MessageBuilderConfig MessageCreation { get; set; }

        /// <summary>The transmitter used to send messages to the Syslog server</summary>
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