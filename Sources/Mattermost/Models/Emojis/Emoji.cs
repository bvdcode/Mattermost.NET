using System.Text.Json.Serialization;

namespace Mattermost.Models.Emojis
{
    /// <summary>
    /// Metadata for a custom emoji.
    /// </summary>
    public class Emoji
    {
        /// <summary>
        /// Emoji identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Creation time as Unix milliseconds.
        /// </summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// Last update time as Unix milliseconds.
        /// </summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>
        /// Deletion time as Unix milliseconds, or zero for an active emoji.
        /// </summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>
        /// Identifier of the user who created the emoji.
        /// </summary>
        [JsonPropertyName("creator_id")]
        public string CreatorId { get; set; } = null!;

        /// <summary>
        /// Emoji name without surrounding colons.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}
