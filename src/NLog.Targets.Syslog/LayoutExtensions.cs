using System.Runtime.CompilerServices;
using NLog.Layouts;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    public static class LayoutExtensions
    {
        /// <summary>Renders a layout truncated at <paramref name="maxLength"/> characters</summary>
        /// <param name="layout">The layout to render</param>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="maxLength">Maximum number of characters</param>
        /// <returns>String that contains at most <paramref name="maxLength" /> characters</returns>
        public static string Render(this Layout layout, LogEventInfo logEvent, int maxLength)
        {
            return Left(layout.Render(logEvent), maxLength);
        }

        /// <summary>Gets at most the first <paramref name="maxLength"/> characters</summary>
        /// <param name="value">Source string</param>
        /// <param name="maxLength">Maximum number of characters</param>
        /// <returns>String that contains at most <paramref name="maxLength" /> characters</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Left(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}