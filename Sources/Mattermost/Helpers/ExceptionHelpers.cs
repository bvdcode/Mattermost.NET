using System;

namespace Mattermost.Helpers
{
    internal class ExceptionHelpers
    {
        internal static void ThrowIfEmpty(string text, string paramName)
        {
            ThrowIfEmpty(paramName, nameof(paramName));
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException(paramName + " cannot be null or empty.", paramName);
            }
        }
    }
}
