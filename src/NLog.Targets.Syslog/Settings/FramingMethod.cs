// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>The framing method to be used when transmitting a message</summary>
    public enum FramingMethod
    {
        /// <summary>NonTransparent framing</summary>
        NonTransparent,

        /// <summary>OctetCounting framing</summary>
        OctetCounting
    }
}