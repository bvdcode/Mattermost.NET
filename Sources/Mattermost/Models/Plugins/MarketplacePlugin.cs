using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Plugins
{
    /// <summary>A Marketplace plugin with its installed version, when present.</summary>
    public class MarketplacePlugin
    {
        /// <summary>Homepage URL.</summary>
        [JsonPropertyName("homepage_url")]
        public string HomepageUrl { get; set; } = string.Empty;

        /// <summary>Icon data.</summary>
        [JsonPropertyName("icon_data")]
        public string IconData { get; set; } = string.Empty;

        /// <summary>Download URL.</summary>
        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>Release notes URL.</summary>
        [JsonPropertyName("release_notes_url")]
        public string ReleaseNotesUrl { get; set; } = string.Empty;

        /// <summary>Optional Marketplace labels.</summary>
        [JsonPropertyName("labels")]
        public IList<MarketplaceLabel>? Labels { get; set; }

        /// <summary>Native hosting restriction.</summary>
        [JsonPropertyName("hosting")]
        public string Hosting { get; set; } = string.Empty;

        /// <summary>Native maintainer category.</summary>
        [JsonPropertyName("author_type")]
        public string AuthorType { get; set; } = string.Empty;

        /// <summary>Native release stage.</summary>
        [JsonPropertyName("release_stage")]
        public string ReleaseStage { get; set; } = string.Empty;

        /// <summary>Whether the Marketplace classifies this as an enterprise plugin.</summary>
        [JsonPropertyName("enterprise")]
        public bool IsEnterprise { get; set; }

        /// <summary>Base64-encoded plugin signature.</summary>
        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;

        /// <summary>Plugin manifest.</summary>
        [JsonPropertyName("manifest")]
        public PluginManifest? Manifest { get; set; }

        /// <summary>Installed version, or empty when not installed.</summary>
        [JsonPropertyName("installed_version")]
        public string InstalledVersion { get; set; } = string.Empty;
    }
}
