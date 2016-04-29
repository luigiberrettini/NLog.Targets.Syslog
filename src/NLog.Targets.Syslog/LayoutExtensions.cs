using NLog.Layouts;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    /// <summary>Allows to extend the behavior of layouts</summary>
    public static class LayoutExtensions
    {
        private const string NilValue = "-";

        /// <summary>Renders a layout truncated at <paramref name="maxLength"/> characters or provides a default value</summary>
        /// <param name="layout">The layout to render</param>
        /// <param name="logEvent">The NLog.LogEventInfo</param>
        /// <param name="maxLength">Maximum number of characters</param>
        /// <param name="defaultValue">The default value in case of an empty rendered layout</param>
        /// <returns>String that contains at most <paramref name="maxLength" /> characters or a default value</returns>
        public static string RenderOrDefault(this Layout layout, LogEventInfo logEvent, int maxLength, string defaultValue = NilValue)
        {
            var renderedLayout = layout.Render(logEvent);
            return renderedLayout.Length == 0 ? NilValue : renderedLayout.Left(maxLength);
        }

        private static string Left(this string str, int maxLength)
        {
            return str.Length <= maxLength ? str : str.Substring(0, maxLength);
        }
    }
}