using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>
    /// A user's unread message and mention counts for a channel.
    /// </summary>
    public class ChannelUnread
    {
        /// <summary>
        /// Team identifier. Direct and group message channels have no team identifier.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = null!;

        /// <summary>
        /// Channel identifier.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = null!;

        /// <summary>
        /// Unread message count.
        /// </summary>
        [JsonPropertyName("msg_count")]
        public long MessageCount { get; set; }

        /// <summary>
        /// Unread mention count.
        /// </summary>
        [JsonPropertyName("mention_count")]
        public long MentionCount { get; set; }

        /// <summary>
        /// Unread root post mention count.
        /// </summary>
        [JsonPropertyName("mention_count_root")]
        public long RootMentionCount { get; set; }

        /// <summary>
        /// Unread urgent mention count.
        /// </summary>
        [JsonPropertyName("urgent_mention_count")]
        public long UrgentMentionCount { get; set; }

        /// <summary>
        /// Unread root post count.
        /// </summary>
        [JsonPropertyName("msg_count_root")]
        public long RootMessageCount { get; set; }
    }
}
