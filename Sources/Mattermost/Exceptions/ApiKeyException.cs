using System;
using System.Runtime.Serialization;

namespace Mattermost.Exceptions
{
    [Serializable]
    internal class ApiKeyException : Exception
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

        protected ApiKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}