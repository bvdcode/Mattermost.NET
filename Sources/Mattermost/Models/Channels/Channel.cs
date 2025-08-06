using System;
using Mattermost.Enums;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>
    /// Channel information.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Channel identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The time in milliseconds a channel was created.
        /// </summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a channel was last updated.
        /// </summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>
        /// The time in milliseconds a channel was deleted.
        /// </summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>
        /// Team identifier who has the channel.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>
        /// Channel type: O (open), P (private), or D (direct).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Channel type as <see cref="ChannelType"/> enum.
        /// </summary>
        [JsonIgnore]
        public ChannelType ChannelType
        {
            get => Type switch
            {
                "O" => ChannelType.Public,
                "P" => ChannelType.Private,
                "D" => ChannelType.Direct,
                _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
            };
            set => Type = value.ToChannelChar() 
                ?? throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }

        /// <summary>
        /// Channel diplay name.
        /// </summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Channel name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Channel header.
        /// </summary>
        [JsonPropertyName("header")]
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Channel purpose.
        /// </summary>
        [JsonPropertyName("purpose")]
        public string Purpose { get; set; } = string.Empty;

        /// <summary>
        /// The time in milliseconds of the last post of a channel.
        /// </summary>
        [JsonPropertyName("last_post_at")]
        public long LastPostAt { get; set; }

        /// <summary>
        /// Total channel messages count.
        /// </summary>
        [JsonPropertyName("total_msg_count")]
        public int TotalMessageCount { get; set; }

        /// <summary>
        /// User identifier who created the channel.
        /// </summary>
        [JsonPropertyName("creator_id")]
        public string CreatorUserId { get; set; } = string.Empty;
    }
}
