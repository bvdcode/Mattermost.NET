﻿using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Post information.
    /// </summary>
    public class Post
    {
        /// <summary>
        /// Post identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The time in milliseconds a post was created.
        /// </summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a post was last updated.
        /// </summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a post was edited.
        /// </summary>
        [JsonPropertyName("edit_at")]
        public long EditedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a post was deleted.
        /// </summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>
        /// True if post is pinned, otherwise false.
        /// </summary>
        [JsonPropertyName("is_pinned")]
        public bool IsPinned { get; set; }

        /// <summary>
        /// User who created the post.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Channel identidier where post was created.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = null!;

        /// <summary>
        /// Root post identidier if the post is child of thread.
        /// </summary>
        [JsonPropertyName("root_id")]
        public string? RootId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("original_id")]
        public string? OriginalId { get; set; }

        /// <summary>
        /// Post text message.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Post hashtags.
        /// </summary>
        [JsonPropertyName("hashtags")]
        public string? Hashtags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pending_post_id")]
        public string? PendingPostId { get; set; }

        /// <summary>
        /// Reply post count (thread messages count).
        /// </summary>
        [JsonPropertyName("reply_count")]
        public int ReplyCount { get; set; }

        /// <summary>
        /// The time in milliseconds when post was replied.
        /// </summary>
        [JsonPropertyName("last_reply_at")]
        public int LastReplyAt { get; set; }

        /// <summary>
        /// Files attached to the post.
        /// </summary>
        [JsonPropertyName("file_ids")]
        public IEnumerable<string> FileIdentifiers { get; set; } = new List<string>();

        /// <summary>
        /// Post metadata.
        /// </summary>
        [JsonPropertyName("props")]
        public Dictionary<string, object> Props { get; set; } = new Dictionary<string, object>();
    }
}