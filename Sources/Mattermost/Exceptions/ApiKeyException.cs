using System;
using System.Runtime.Serialization;

namespace Mattermost.Exceptions
{
    [Serializable]
    internal class ApiKeyException : MattermostClientException
    {
        public ApiKeyException()
        {
        }

        public ApiKeyException(string message) : base(message)
        {
        }

        public ApiKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}