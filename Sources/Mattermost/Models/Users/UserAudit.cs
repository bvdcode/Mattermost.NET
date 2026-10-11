using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>An entry in a user's audit history.</summary>
    public class UserAudit
    {
        /// <summary>Audit entry identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>Event time as a Unix timestamp in milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>User identifier.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>Recorded action.</summary>
        [JsonPropertyName("action")]
        public string Action { get; set; } = null!;

        /// <summary>Additional event information.</summary>
        [JsonPropertyName("extra_info")]
        public string ExtraInfo { get; set; } = null!;

        /// <summary>Client IP address recorded for the event.</summary>
        [JsonPropertyName("ip_address")]
        public string IpAddress { get; set; } = null!;

        /// <summary>Session identifier associated with the event.</summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = null!;
    }
}
