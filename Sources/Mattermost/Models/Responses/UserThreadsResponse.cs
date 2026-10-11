using Mattermost.Models.Posts;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>A chunk of followed threads and aggregate unread counts.</summary>
    public class UserThreadsResponse
    {
        /// <summary>Total followed threads, not the number in this chunk.</summary>
        [JsonPropertyName("total")]
        public long Total { get; set; }

        /// <summary>Total threads with unread replies.</summary>
        [JsonPropertyName("total_unread_threads")]
        public long TotalUnreadThreads { get; set; }

        /// <summary>Total unread mentions.</summary>
        [JsonPropertyName("total_unread_mentions")]
        public long TotalUnreadMentions { get; set; }

        /// <summary>Total unread urgent mentions.</summary>
        [JsonPropertyName("total_unread_urgent_mentions")]
        public long TotalUnreadUrgentMentions { get; set; }

        /// <summary>Threads in this chunk. The server may return null when there are none.</summary>
        [JsonPropertyName("threads")]
        public IList<UserThread>? Threads { get; set; } = new List<UserThread>();
    }
}
