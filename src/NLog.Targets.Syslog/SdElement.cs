using NLog.Config;
using NLog.Layouts;
using System.Collections.Generic;
using System.Text;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>A Syslog SD-ELEMENT field</summary>
    [NLogConfigurationItem]
    public class SdElement
    {
        private readonly byte[] leftBracketBytes = Encoding.ASCII.GetBytes("[");
        private readonly byte[] spaceBytes = Encoding.ASCII.GetBytes(" ");
        private readonly byte[] rightBracketBytes = Encoding.ASCII.GetBytes("]");

        /// <summary>The SD-ID field of an SD-ELEMENT field in the STRUCTURED-DATA part of the message</summary>
        public Layout SdId { get; set; }

        /// <summary>The SD-PARAM fields belonging to an SD-ELEMENT field in the STRUCTURED-DATA part of the message</summary>
        [ArrayParameter(typeof(SdParam), nameof(SdParam))]
        public IList<SdParam> SdParams { get; set; }

        /// <summary>Initializes a new instance of the SdElement class</summary>
        public SdElement(Layout sender, Layout hostname)
        {
            SdParams = new List<SdParam>();
        }

        /// <summary>Gives the binary representation of this SD-ELEMENT field</summary>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <returns>Byte array containing this SD-ELEMENT field</returns>
        public IEnumerable<byte> Bytes(LogEventInfo logEvent)
        {
            var sdElementBytes = new List<byte>();
            sdElementBytes.AddRange(leftBracketBytes);
            sdElementBytes.AddRange(SdIdBytes(logEvent));
            sdElementBytes.AddRange(SdParamsBytes(logEvent));
            sdElementBytes.AddRange(rightBracketBytes);
            return sdElementBytes.ToArray();
        }

        private IEnumerable<byte> SdIdBytes(LogEventInfo logEvent)
        {
            var sdId = SdId.Render(logEvent);
            return Encoding.ASCII.GetBytes(sdId);
        }

        private IEnumerable<byte> SdParamsBytes(LogEventInfo logEvent)
        {
            var sdParamsBytes = new List<byte>();
            SdParams.ForEach(sdParam =>
            {
                sdParamsBytes.AddRange(spaceBytes);
                sdParamsBytes.AddRange(sdParam.Bytes(logEvent));
            });
            return sdParamsBytes.ToArray();
        }
    }

    /*
        SDELEMENT = [ SDID (one or more SPACE PARAMNAME = " PARAMVALUE ") ]
            SDID
                At most 32 SAFEPRINTUSASCII specifying a unique identifier within STRUCTUREDDATA
                The identifier can be a CUSTOMID or IANAID:
                    CUSTOMID = NAME @ PEN
                        NAME
                            SAFEPRINTUSASCII except @
                        PEN
                            A private enterprise number
                            Digits or digits separated by periods
                    IANAID = TIMEQUALITY or ORIGIN or META
                        TIMEQUALITY
                            Parameters are tzKnown, isSynced, syncAccuracy
                        ORIGIN
                            Parameters are ip, enterpriseId, software, swVersion
                        META
                            Parameters are sequenceId, sysUpTime, language
            PARAMNAME
                1 to 32 SAFEPRINTUSASCII
            PARAMVALUE
                UTF8 STRING with ", \ and ] escaped as \", \\ and \]
    */
}