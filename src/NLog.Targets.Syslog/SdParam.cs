using NLog.Config;
using NLog.Layouts;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>A Syslog SD-PARAM field</summary>
    [NLogConfigurationItem]
    public class SdParam
    {
        private static readonly byte[] EqualBytes = Encoding.ASCII.GetBytes("=");
        private static readonly byte[] QuotesBytes = Encoding.ASCII.GetBytes("\"");

        /// <summary>The PARAM-NAME field of this SD-PARAM</summary>
        public Layout Name { get; set; }

        /// <summary>The PARAM-VALUE field of this SD-PARAM</summary>
        public Layout Value { get; set; }

        /// <summary>Gives the binary representation of this SD-PARAM field</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <returns>Bytes containing this SD-PARAM field</returns>
        public IEnumerable<byte> Bytes(LogEventInfo logEvent)
        {
            return NameBytes(logEvent)
                .Concat(EqualBytes)
                .Concat(QuotesBytes)
                .Concat(ValueBytes(logEvent))
                .Concat(QuotesBytes);
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