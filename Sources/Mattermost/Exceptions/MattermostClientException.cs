using System;

namespace Mattermost.Exceptions
{
    internal class MattermostClientException : Exception
    {
        public MattermostClientException()
        {
        }

        public MattermostClientException(string message) : base(message)
        {
        }

        public MattermostClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}