using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>A server-synchronized draft belonging to the authenticated user.</summary>
    public class Draft
    {
        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time in Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deprecated deletion time. Drafts are hard-deleted.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Owner identifier, assigned by the server.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Channel identifier.</summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>Root post identifier for a reply draft, or empty for a channel draft.</summary>
        [JsonPropertyName("root_id")]
        public string RootId { get; set; } = string.Empty;

        /// <summary>Draft message. Saving an empty message deletes the draft.</summary>
        [JsonPropertyName("message")]
        public string Text { get; set; } = string.Empty;

        /// <summary>Post type, or empty for a normal message.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Extensible post properties.</summary>
        [JsonPropertyName("props")]
        public IDictionary<string, object>? Props { get; set; } = new Dictionary<string, object>();

        /// <summary>Identifiers of attached files.</summary>
        [JsonPropertyName("file_ids")]
        public IList<string>? FileIdentifiers { get; set; } = new List<string>();

        /// <summary>Server-provided post metadata, including attached file information.</summary>
        [JsonPropertyName("metadata")]
        public IDictionary<string, object>? Metadata { get; set; }

        /// <summary>Priority properties: priority, requested_ack and persistent_notifications.</summary>
        [JsonPropertyName("priority")]
        public IDictionary<string, object>? Priority { get; set; }
    }
}
