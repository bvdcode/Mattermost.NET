using System;

namespace Mattermost.Exceptions
{
    /// <summary>
    /// Thrown when the API key is invalid.
    /// </summary>
    [Serializable]
    public class ApiKeyException : MattermostClientException
    {
        internal ApiKeyException() { }

        internal ApiKeyException(string message) : base(message) { }

        internal ApiKeyException(string message, Exception innerException) : base(message, innerException) { }
    }
}