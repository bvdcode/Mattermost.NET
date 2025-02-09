using System.Text.Json;
using Mattermost.Models.Posts;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses.Websocket.Posts
{
    /// <summary>
    /// Base post information class.
    /// </summary>
    public class PostInfo
    {
        /// <summary>
        /// Channel display name.
        /// </summary>
        [JsonPropertyName("channel_display_name")]
        public string ChannelDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Channel name.
        /// </summary>
        [JsonPropertyName("channel_name")]
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// Channel type: O (open) or P (private).
        /// </summary>
        [JsonPropertyName("channel_type")]
        public string ChannelType { get; set; } = string.Empty;

        /// <summary>
        /// Post mentions.
        /// </summary>
        [JsonPropertyName("mentions")]
        public string Mentions { get; set; } = string.Empty;

        /// <summary>
        /// Raw post JSON data.
        /// </summary>
        [JsonPropertyName("post")]
        public string RawPostData { get; set; } = string.Empty;

        /// <summary>
        /// Post data.
        /// </summary>
        public Post Post
        {
            get
            {
                parsedPost ??= JsonSerializer.Deserialize<Post>(RawPostData)
                        ?? throw new JsonException("Failed to deserialize post data.");
                return parsedPost;
            }
        }

        private Post? parsedPost = null!;

        /// <summary>
        /// Post sender name.
        /// </summary>
        [JsonPropertyName("sender_name")]
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// Team identifier of channel.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>
        /// Set online when post created.
        /// </summary>
        [JsonPropertyName("set_online")]
        public bool SetOnline { get; set; }
    }
}