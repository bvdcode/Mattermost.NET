using System;

namespace Mattermost.Exceptions
{
    /// <summary>
    /// Thrown when credentials are invalid or the user is not authorized to perform the requested operation.
    /// </summary>
    public class AuthorizationException : MattermostClientException
    {
        internal AuthorizationException() { }

        internal AuthorizationException(string message) : base(message) { }

        internal AuthorizationException(string message, Exception innerException) : base(message, innerException) { }
    }
}