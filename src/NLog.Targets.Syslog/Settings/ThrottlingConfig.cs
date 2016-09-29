namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Throttling configuration</summary>
    public class ThrottlingConfig
    {
        /// <summary>The number of log entries, waiting to be processed, that triggers throttling</summary>
        public int Limit { get; set; }

        /// <summary>The throttling strategy to employ</summary>
        public ThrottlingStrategy Strategy { get; set; }

        /// <summary>The milliseconds/percentage delay for a DiscardOnFixedTimeout/DiscardOnPercentageTimeout/Defer throttling strategy</summary>
        public decimal Delay { get; set; }

        /// <summary>Builds a new instance of the Throttling class</summary>
        public ThrottlingConfig()
        {
            Limit = 0;
            Strategy = ThrottlingStrategy.None;
            Delay = 0;
        }

        internal void EnsureAllowedValues()
        {
            if (Limit < 1)
                Limit = 0;
            if (Delay < 0)
                Delay = 0;
            if (Limit == 0)
                Strategy = ThrottlingStrategy.None;
        }
    }
}