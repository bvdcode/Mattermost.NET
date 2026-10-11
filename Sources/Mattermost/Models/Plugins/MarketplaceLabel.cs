using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>A label displayed for a Marketplace plugin.</summary>
    public class MarketplaceLabel
    {
        /// <summary>Label name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Label description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Related URL.</summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>Native label color.</summary>
        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;
    }
}
