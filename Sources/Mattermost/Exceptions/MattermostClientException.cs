using System;
using System.Net;

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

        /// <summary>
        /// Gets the HTTP status code returned by the Mattermost server.
        /// </summary>
        public HttpStatusCode StatusCode { get; internal set; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets the JSON response from the Mattermost server.
        /// </summary>
        public string ResponseJson { get; internal set; } = "{}";

        /// <summary>
        /// Gets the request URI that caused the exception.
        /// </summary>
        public string RequestUri { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets the HTTP method used for the request that caused the exception.
        /// </summary>
        public string RequestMethod { get; internal set; } = string.Empty;
    }
}