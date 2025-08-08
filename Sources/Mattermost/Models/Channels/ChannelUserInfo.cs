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
        /// The time in milliseconds the channel was last viewed by the user.
        /// </summary>
        [JsonPropertyName("last_viewed_at")]
        public int LastViewedAt { get; set; }

        /// <summary>
        /// How many messages are posted in the channel by user.
        /// </summary>
        [JsonPropertyName("msg_count")]
        public int MessageCount { get; set; }

        /// <summary>
        /// Mentions count in channel by user.
        /// </summary>
        [JsonPropertyName("mention_count")]
        public int MentionCount { get; set; }

        /// <summary>
        /// Notify props for user in channel.
        /// </summary>
        [JsonPropertyName("notify_props")]
        public NotifyProps NotifyProps { get; set; } = new NotifyProps();

        /// <summary>
        /// Last update time in milliseconds.
        /// </summary>
        [JsonPropertyName("last_update_at")]
        public long UpdatedAt { get; set; }
    }
}