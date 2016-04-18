using NLog.Layouts;
using System.Runtime.CompilerServices;

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
            return layout.Render(logEvent).Left(maxLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Left(this string str, int maxLength)
        {
            return str.Length <= maxLength ? str : str.Substring(0, maxLength);
        }
    }
}