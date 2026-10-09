using Mattermost.Models.Enums;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>
    /// A user's presence status and activity metadata.
    /// </summary>
    public class UserPresence
    {
        /// <summary>
        /// User identifier.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Current presence status.
        /// </summary>
        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus Status { get; set; }

        /// <summary>
        /// Whether the user set the status manually.
        /// </summary>
        [JsonPropertyName("manual")]
        public bool IsManual { get; set; }

        /// <summary>
        /// Last activity time as a Unix timestamp in milliseconds.
        /// </summary>
        [JsonPropertyName("last_activity_at")]
        public long LastActivityAt { get; set; }

        /// <summary>
        /// Do not disturb expiry as a Unix timestamp in seconds, or zero when no expiry is set.
        /// </summary>
        [JsonPropertyName("dnd_end_time")]
        public long DoNotDisturbEndTime { get; set; }
    }
}
