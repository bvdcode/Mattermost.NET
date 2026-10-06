using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>
    /// Channel user information.
    /// </summary>
    public class ChannelUserInfo
    {
        /// <summary>
        /// Channel identifier.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// User identifier.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// User roles in channel.
        /// </summary>
        [JsonPropertyName("roles")]
        public string Roles { get; set; } = string.Empty;

        /// <summary>
        /// The time in milliseconds the channel was last viewed by the user, or -1 when hidden by the server.
        /// </summary>
        [JsonPropertyName("last_viewed_at")]
        public long LastViewedAt { get; set; }

        /// <summary>
        /// The member's message counter in the channel.
        /// </summary>
        [JsonPropertyName("msg_count")]
        public long MessageCount { get; set; }

        /// <summary>
        /// The member's mention counter in the channel.
        /// </summary>
        [JsonPropertyName("mention_count")]
        public long MentionCount { get; set; }

        /// <summary>
        /// Notify props for user in channel.
        /// </summary>
        [JsonPropertyName("notify_props")]
        public NotifyProps NotifyProps { get; set; } = new NotifyProps();

        /// <summary>
        /// Last update time in milliseconds, or -1 when hidden by the server.
        /// </summary>
        [JsonPropertyName("last_update_at")]
        public long UpdatedAt { get; set; }
    }
}
