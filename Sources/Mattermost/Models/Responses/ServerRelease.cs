using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Mattermost server release metadata returned by the server.</summary>
    public class ServerRelease
    {
        /// <summary>Release identifier.</summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>Release tag.</summary>
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        /// <summary>Release name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Creation timestamp in the native string format.</summary>
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>Publication timestamp in the native string format.</summary>
        [JsonPropertyName("published_at")]
        public string PublishedAt { get; set; } = string.Empty;

        /// <summary>Release notes.</summary>
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        /// <summary>Release page URL.</summary>
        [JsonPropertyName("html_url")]
        public string Url { get; set; } = string.Empty;
    }
}
