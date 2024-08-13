using System;

namespace Mattermost.Exceptions
{
    /// <summary>
    /// Base class for all exceptions thrown by the Mattermost client.
    /// </summary>
    public class MattermostClientException : Exception
    {
        internal MattermostClientException() { }

        internal MattermostClientException(string message) : base(message) { }

        internal MattermostClientException(string message, Exception innerException) : base(message, innerException) { }
    }
}