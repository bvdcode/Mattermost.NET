using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>Read boundary returned when marking a channel unread from a post.</summary>
    public class ChannelReadState
    {
        /// <summary>Team identifier; empty for direct and group channels.</summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>User whose read state was updated.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Channel whose read state was updated.</summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>Channel message counter at the read boundary, not the number of unread messages.</summary>
        [JsonPropertyName("msg_count")]
        public long MessageCount { get; set; }

        /// <summary>Unread mention count.</summary>
        [JsonPropertyName("mention_count")]
        public long MentionCount { get; set; }

        /// <summary>Unread root-post mention count.</summary>
        [JsonPropertyName("mention_count_root")]
        public long RootMentionCount { get; set; }

        /// <summary>Unread urgent mention count.</summary>
        [JsonPropertyName("urgent_mention_count")]
        public long UrgentMentionCount { get; set; }

        /// <summary>Root-post counter at the read boundary, not the number of unread root posts.</summary>
        [JsonPropertyName("msg_count_root")]
        public long RootMessageCount { get; set; }

        /// <summary>Updated read boundary as Unix milliseconds.</summary>
        [JsonPropertyName("last_viewed_at")]
        public long LastViewedAt { get; set; }
    }
}
