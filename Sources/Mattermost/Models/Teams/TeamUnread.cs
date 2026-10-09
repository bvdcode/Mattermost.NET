using System.Text.Json.Serialization;

namespace Mattermost.Models.Teams
{
    /// <summary>
    /// A user's unread message, mention and thread counts for a team.
    /// </summary>
    public class TeamUnread
    {
        /// <summary>
        /// Team identifier.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = null!;

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
        /// Unread root post count.
        /// </summary>
        [JsonPropertyName("msg_count_root")]
        public long RootMessageCount { get; set; }

        /// <summary>
        /// Unread followed thread count when collapsed threads are included.
        /// </summary>
        [JsonPropertyName("thread_count")]
        public long ThreadCount { get; set; }

        /// <summary>
        /// Unread followed thread mention count when collapsed threads are included.
        /// </summary>
        [JsonPropertyName("thread_mention_count")]
        public long ThreadMentionCount { get; set; }

        /// <summary>
        /// Unread urgent mention count in followed threads when collapsed threads are included.
        /// </summary>
        [JsonPropertyName("thread_urgent_mention_count")]
        public long ThreadUrgentMentionCount { get; set; }
    }
}
