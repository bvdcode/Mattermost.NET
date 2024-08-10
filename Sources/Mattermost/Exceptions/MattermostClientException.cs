using System;

namespace Mattermost.Exceptions
{
    public class MattermostClientException : Exception
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