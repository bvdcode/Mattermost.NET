using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>
    /// Represents details of an error returned by the Mattermost API.
    /// </summary>
    public class MattermostApiErrorDetails
    {
        /// <summary>
        /// Error identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Description of the error.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error message, if available.
        /// </summary>
        [JsonPropertyName("detailed_error")]
        public string DetailedError { get; set; } = string.Empty;

        /// <summary>
        /// Request ID associated with the error, useful for debugging.
        /// </summary>
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code associated with the error.
        /// </summary>
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
    }
}
