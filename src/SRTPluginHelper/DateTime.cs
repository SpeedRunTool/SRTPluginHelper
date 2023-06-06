using System;

namespace SRTPluginHelper
{
    /// <summary>
    /// A static helper class with various methods for handling dates and times.
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        /// Gets the current date and time as UTC with the supplied format string or identifier.
        /// </summary>
        /// <param name="format">A DateTimeOffset format string or identifier.</param>
        /// <returns>The current date and time as UTC with the supplied format string or identifier.</returns>
        public static string GetCurrentUtcString(string format = "u") => DateTimeOffset.UtcNow.ToString(format);
    }
}
