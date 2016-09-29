namespace NLog.Targets.Syslog.Settings
{
    /// <summary>The framing method to be used when transmitting a message</summary>
    public enum FramingMethod
    {
        NonTransparent,
        OctetCounting
    }
}