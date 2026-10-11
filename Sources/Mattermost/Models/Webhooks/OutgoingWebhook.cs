using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Webhooks
{
    /// <summary>Outgoing webhook configuration.</summary>
    public class OutgoingWebhook
    {
        /// <summary>Webhook identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Secret sent to callback receivers. Must be preserved when updating the webhook.</summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

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
        [JsonPropertyName("creator_id")]
        public string CreatorId { get; set; } = string.Empty;

        /// <summary>Public channel identifier, or empty to match public channels across the team.</summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>Team identifier.</summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>Trigger words. Team-wide webhooks require at least one.</summary>
        [JsonPropertyName("trigger_words")]
        public IList<string>? TriggerWords { get; set; } = new List<string>();

        /// <summary>How the first word of a message matches a trigger word.</summary>
        [JsonPropertyName("trigger_when")]
        public OutgoingWebhookTriggerWhen TriggerWhen { get; set; }

        /// <summary>HTTP or HTTPS callback URLs.</summary>
        [JsonPropertyName("callback_urls")]
        public IList<string> CallbackUrls { get; set; } = new List<string>();

        /// <summary>Webhook display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Webhook description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Callback content type: application/json or application/x-www-form-urlencoded.</summary>
        [JsonPropertyName("content_type")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>Posting username override, subject to server settings.</summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>Posting icon URL override, subject to server settings.</summary>
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;
    }
}
