using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.OAuth
{
    /// <summary>An OAuth client application registered with Mattermost.</summary>
    public class OAuthApp
    {
        /// <summary>Application client identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Identifier of the account that registered the application.</summary>
        [JsonPropertyName("creator_id")]
        public string CreatorId { get; set; } = string.Empty;

        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time in Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Client secret. Empty for public clients and information-only or authorized-app responses.</summary>
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>Application display name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Application description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Application icon URL, or an empty string.</summary>
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;

        /// <summary>Allowed OAuth redirect URLs, in their configured order.</summary>
        [JsonPropertyName("callback_urls")]
        public IList<string> CallbackUrls { get; set; } = new List<string>();

        /// <summary>Application homepage URL.</summary>
        [JsonPropertyName("homepage")]
        public string Homepage { get; set; } = string.Empty;

        /// <summary>Whether users can authorize the application without a consent prompt.</summary>
        [JsonPropertyName("is_trusted")]
        public bool IsTrusted { get; set; }

        /// <summary>Associated Mattermost Apps identifier, or an empty string.</summary>
        [JsonPropertyName("mattermost_app_id")]
        public string MattermostAppId { get; set; } = string.Empty;

        /// <summary>Whether the application was created through dynamic client registration.</summary>
        [JsonPropertyName("is_dynamically_registered")]
        public bool IsDynamicallyRegistered { get; set; }
    }
}
