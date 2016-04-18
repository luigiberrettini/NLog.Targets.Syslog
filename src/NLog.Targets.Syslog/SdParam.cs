using NLog.Config;
using NLog.Layouts;
using System.Collections.Generic;
using System.Text;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>A Syslog SD-PARAM field</summary>
    [NLogConfigurationItem]
    public class SdParam
    {
        private readonly byte[] equalBytes = Encoding.ASCII.GetBytes("=");
        private readonly byte[] quotesBytes = Encoding.ASCII.GetBytes("\"");

        /// <summary>The PARAM-NAME field of this SD-PARAM</summary>
        public Layout Name { get; set; }

        /// <summary>The PARAM-VALUE field of this SD-PARAM</summary>
        public Layout Value { get; set; }

        /// <summary>Gives the binary representation of this SD-PARAM field</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <returns>Byte array containing this SD-PARAM field</returns>
        public IEnumerable<byte> Bytes(LogEventInfo logEvent)
        {
            var sdParamBytes = new List<byte>();
            sdParamBytes.AddRange(NameBytes(logEvent));
            sdParamBytes.AddRange(equalBytes);
            sdParamBytes.AddRange(quotesBytes);
            sdParamBytes.AddRange(ValueBytes(logEvent));
            sdParamBytes.AddRange(quotesBytes);
            return sdParamBytes.ToArray();
        }

        private IEnumerable<byte> NameBytes(LogEventInfo logEvent)
        {
            var paramName = Name.Render(logEvent);
            return Encoding.ASCII.GetBytes(paramName);
        }
        private IEnumerable<byte> ValueBytes(LogEventInfo logEvent)
        {
            var paramValue = Value.Render(logEvent);
            return Encoding.UTF8.GetBytes(paramValue);
        }
    }
}