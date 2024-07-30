using System.Text.Json;
using Mattermost.Models.Posts;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses.Websocket
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
        /// Post raw JSON data.
        /// </summary>
        [JsonPropertyName("post")]
        public string Post { get; set; } = string.Empty;

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

        internal PostUpdateInfo ToPostUpdateInfo()
        {
            return new PostUpdateInfo()
            {
                ChannelDisplayName = ChannelDisplayName,
                ChannelName = ChannelName,
                ChannelType = ChannelType,
                Mentions = Mentions,
                Post = JsonSerializer.Deserialize<Post>(Post)!,
                SenderName = SenderName,
                SetOnline = SetOnline,
                TeamId = TeamId
            };
        }
    }
}