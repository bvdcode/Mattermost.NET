using System;

namespace Mattermost.Helpers
{
    /// <summary>
    /// Helper class for throwing exceptions.
    /// </summary>
    public class ExceptionHelpers
    {
        /// <summary>
        /// Throws an ArgumentNullException if the given string is null or empty.
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown when the string is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the parameter name is null or empty.</exception>
        public static void ThrowIfEmpty(string text, string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
            {
                throw new ArgumentNullException(nameof(paramName), "Parameter name cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException(paramName + " cannot be null or empty.", paramName);
            }
        }
    }
}
