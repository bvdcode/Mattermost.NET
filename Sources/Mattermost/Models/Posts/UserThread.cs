using Mattermost.Models.Users;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>A followed thread, its root post and the user's unread state.</summary>
    public class UserThread
    {
        /// <summary>Root post identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Number of non-deleted replies.</summary>
        [JsonPropertyName("reply_count")]
        public long ReplyCount { get; set; }

        /// <summary>Most recent reply time in Unix milliseconds.</summary>
        [JsonPropertyName("last_reply_at")]
        public long LastReplyAt { get; set; }

        /// <summary>User's last viewed time in Unix milliseconds.</summary>
        [JsonPropertyName("last_viewed_at")]
        public long LastViewedAt { get; set; }

        /// <summary>Participants. Profiles contain additional details when extended is requested.</summary>
        [JsonPropertyName("participants")]
        public IList<User>? Participants { get; set; } = new List<User>();

        /// <summary>Root post.</summary>
        [JsonPropertyName("post")]
        public Post? Post { get; set; }

        /// <summary>Number of unread replies.</summary>
        [JsonPropertyName("unread_replies")]
        public long UnreadReplies { get; set; }

        /// <summary>Number of unread mentions.</summary>
        [JsonPropertyName("unread_mentions")]
        public long UnreadMentions { get; set; }

        /// <summary>Whether the thread is urgent.</summary>
        [JsonPropertyName("is_urgent")]
        public bool IsUrgent { get; set; }

        /// <summary>Root post deletion time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }
    }
}
