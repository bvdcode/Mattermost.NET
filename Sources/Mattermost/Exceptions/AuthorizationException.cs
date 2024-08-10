using System;

namespace Mattermost.Exceptions
{
    internal class  AuthorizationException : MattermostClientException
    {
        public AuthorizationException()
        {
        }

        public AuthorizationException(string message) : base(message)
        {
        }

        public AuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}