using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Post reaction information.
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// User identifier that created this reaction.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Post identifier this reaction belongs to.
        /// </summary>
        [JsonPropertyName("post_id")]
        public string PostId { get; set; } = string.Empty;

        /// <summary>
        /// Emoji name used for the reaction.
        /// </summary>
        [JsonPropertyName("emoji_name")]
        public string EmojiName { get; set; } = string.Empty;

        /// <summary>
        /// The time in milliseconds this reaction was created.
        /// </summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }
    }
}
