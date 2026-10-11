using System.Text.Json.Serialization;

namespace Mattermost.Models.Webhooks
{
    /// <summary>Incoming webhook configuration. Its identifier is a secret used in the webhook URL.</summary>
    public class IncomingWebhook
    {
        /// <summary>Webhook identifier. Keep it secret.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time in Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deletion time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Owner identifier. Assigning a different owner on creation requires additional permissions.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Default destination channel identifier.</summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>Team identifier, derived from the destination channel.</summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>Webhook display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Webhook description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Posting username override, subject to server settings.</summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>Posting icon URL override, subject to server settings.</summary>
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;

        /// <summary>Whether posts are restricted to the destination channel. The server may enforce this.</summary>
        [JsonPropertyName("channel_locked")]
        public bool ChannelLocked { get; set; }

        /// <summary>Last use time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("last_used")]
        public long LastUsedAt { get; set; }
    }
}
